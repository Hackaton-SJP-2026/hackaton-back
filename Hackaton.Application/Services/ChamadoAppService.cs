using FluentValidation;
using Hackaton.Application.DTOs;
using Hackaton.Application.Exceptions;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using MapsterMapper;

namespace Hackaton.Application.Services;

public sealed class ChamadoAppService : AppService<Chamado, ChamadoDto>
{
    private readonly IDomainService<Usuario> _usuarios;
    private readonly IDomainService<UnidadeAtendimento> _unidades;
    private readonly IDomainService<EventoChamado> _eventos;
    private readonly IDomainService<TecnicoUnidadeAtendimento> _tecnicoUnidades;
    private readonly IDomainService<Tecnico> _tecnicos;

    public ChamadoAppService(
        IDomainService<Chamado> chamados,
        IDomainService<Usuario> usuarios,
        IDomainService<UnidadeAtendimento> unidades,
        IDomainService<EventoChamado> eventos,
        IDomainService<TecnicoUnidadeAtendimento> tecnicoUnidades,
        IDomainService<Tecnico> tecnicos,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEnumerable<IValidator<ChamadoDto>> validators)
        : base(chamados, unitOfWork, mapper, validators)
    {
        _usuarios = usuarios;
        _unidades = unidades;
        _eventos = eventos;
        _tecnicoUnidades = tecnicoUnidades;
        _tecnicos = tecnicos;
    }

    public override async Task<ChamadoDto> CreateAsync(ChamadoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarAsync(dto, cancellationToken);
        await ValidarReferenciasAsync(dto, cancellationToken);
        var chamado = _mapper.Map<Chamado>(dto);
        chamado.Id = Guid.NewGuid();
        chamado.NivelComplexidade = dto.NivelComplexidade;
        chamado.Pontos = PontuacaoChamadoCalculator.Calcular(dto.NivelComplexidade, dto.Prioridade);
        chamado.Status = Hackaton.Domain.Enums.StatusChamado.Aberto;
        chamado.EtapaAtendimento = "Aberto";
        chamado.TecnicoResponsavelId = null;
        chamado.InicioAtendimento = null;

        var vinculos = await _tecnicoUnidades.GetAllAsync(cancellationToken);
        var vinculoResponsavel = vinculos.FirstOrDefault(x => x.UnidadeAtendimentoId == chamado.UnidadeAtendimentoId
            && x.Ativo && x.Responsavel);
        if (vinculoResponsavel is not null)
        {
            var tecnico = await _tecnicos.GetByIdAsync(vinculoResponsavel.TecnicoId, cancellationToken);
            var usuarios = await _usuarios.GetAllAsync(cancellationToken);
            var usuarioTecnico = tecnico is null ? null : usuarios.FirstOrDefault(x => x.Id == tecnico.UsuarioId);
            if (tecnico is not null && tecnico.Disponibilidade == "Disponivel" && usuarioTecnico is { Ativo: true })
            {
                chamado.TecnicoResponsavelId = tecnico.Id;
                chamado.Status = Hackaton.Domain.Enums.StatusChamado.EmAnalise;
                chamado.EtapaAtendimento = "Vinculado";
            }
        }

        _eventos.Add(new EventoChamado
        {
            ChamadoId = chamado.Id,
            Tipo = "Abertura",
            Descricao = "Chamado aberto no sistema.",
            DataHora = DateTime.UtcNow
        });
        if (chamado.TecnicoResponsavelId.HasValue)
        {
            _eventos.Add(new EventoChamado
            {
                ChamadoId = chamado.Id,
                Tipo = "VinculacaoTecnico",
                Descricao = "Chamado vinculado automaticamente ao técnico responsável pela unidade de atendimento.",
                DataHora = DateTime.UtcNow
            });
        }
        _domainService.Add(chamado);
        await _unitOfWork.Confirm(cancellationToken);
        return _mapper.Map<ChamadoDto>(chamado);
    }

    public override async Task<ChamadoDto> UpdateAsync(Guid id, ChamadoDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarReferenciasAsync(dto, cancellationToken);
        var dtoComPontosCalculados = dto with
        {
            Pontos = PontuacaoChamadoCalculator.Calcular(dto.NivelComplexidade, dto.Prioridade)
        };
        return await base.UpdateAsync(id, dtoComPontosCalculados, cancellationToken);
    }

    private async Task ValidarReferenciasAsync(ChamadoDto dto, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(dto.SolicitanteId, cancellationToken);
        if (usuario is null || !usuario.Ativo)
            throw new EntityValidationException("Solicitante não encontrado ou inativo.");

        var unidade = await _unidades.GetByIdAsync(dto.UnidadeAtendimentoId, cancellationToken);
        if (unidade is null || !unidade.Ativa)
            throw new EntityValidationException("Unidade de atendimento não encontrada ou inativa.");
        if (unidade.PrefeituraId != usuario.PrefeituraId)
            throw new EntityValidationException("Solicitante e unidade devem pertencer à mesma prefeitura.");
    }
}

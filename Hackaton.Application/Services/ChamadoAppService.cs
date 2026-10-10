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

    public ChamadoAppService(
        IDomainService<Chamado> chamados,
        IDomainService<Usuario> usuarios,
        IDomainService<UnidadeAtendimento> unidades,
        IDomainService<EventoChamado> eventos,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEnumerable<IValidator<ChamadoDto>> validators)
        : base(chamados, unitOfWork, mapper, validators)
    {
        _usuarios = usuarios;
        _unidades = unidades;
        _eventos = eventos;
    }

    public override async Task<ChamadoDto> CreateAsync(ChamadoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await ValidarAsync(dto, cancellationToken);
        await ValidarReferenciasAsync(dto, cancellationToken);
        var chamado = _mapper.Map<Chamado>(dto);
        chamado.Id = Guid.NewGuid();
        _eventos.Add(new EventoChamado
        {
            ChamadoId = chamado.Id,
            Tipo = "Abertura",
            Descricao = "Chamado aberto no sistema.",
            DataHora = DateTime.UtcNow
        });
        _domainService.Add(chamado);
        await _unitOfWork.Confirm(cancellationToken);
        return _mapper.Map<ChamadoDto>(chamado);
    }

    public override async Task<ChamadoDto> UpdateAsync(Guid id, ChamadoDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarReferenciasAsync(dto, cancellationToken);
        return await base.UpdateAsync(id, dto, cancellationToken);
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

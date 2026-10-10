using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;
using Hackaton.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackatonApi.Controllers;

[ApiController]
[Route("api/gestor/tecnicos")]
[Authorize(Roles = "Gestor,Admin")]
public sealed class GestorTecnicosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public GestorTecnicosController(AppDbContext db, IUnitOfWork unitOfWork, IPasswordHasher<Usuario> passwordHasher)
    {
        _db = db;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        var tecnicos = await (from t in _db.Tecnicos.AsNoTracking()
                              join u in _db.Usuarios.AsNoTracking() on t.UsuarioId equals u.Id
                              where u.PrefeituraId == prefeituraId
                              orderby u.Nome
                              select new { TecnicoId = t.Id, UsuarioId = u.Id, u.Nome, u.Email, u.Telefone,
                                  t.FotoUrl, t.Disponibilidade, t.MotivoIndisponibilidade, t.Pontos, t.Patente, u.Ativo })
            .ToListAsync(cancellationToken);
        var ids = tecnicos.Select(x => x.TecnicoId).ToArray();
        var unidades = await (from v in _db.TecnicoUnidadesAtendimento.AsNoTracking()
                              join ua in _db.UnidadesAtendimento.AsNoTracking() on v.UnidadeAtendimentoId equals ua.Id
                              where ids.Contains(v.TecnicoId)
                              orderby ua.Nome
                              select new { v.TecnicoId, UnidadeId = ua.Id, Unidade = ua.Nome, v.Ativo, v.Responsavel })
            .ToListAsync(cancellationToken);
        return Ok(tecnicos.Select(t => new
        {
            t.TecnicoId, t.UsuarioId, t.Nome, t.Email, t.Telefone, t.FotoUrl, t.Disponibilidade,
            t.MotivoIndisponibilidade, t.Pontos, t.Patente, t.Ativo,
            Unidades = unidades.Where(u => u.TecnicoId == t.TecnicoId)
        }));
    }

    [HttpGet("{tecnicoId:guid}")]
    public async Task<IActionResult> Detalhes(Guid tecnicoId, CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        var tecnico = await (from t in _db.Tecnicos.AsNoTracking()
                             join u in _db.Usuarios.AsNoTracking() on t.UsuarioId equals u.Id
                             where t.Id == tecnicoId && u.PrefeituraId == prefeituraId
                             select new { TecnicoId = t.Id, UsuarioId = u.Id, u.Nome, u.Email, u.Telefone,
                                 t.FotoUrl, t.Disponibilidade, t.MotivoIndisponibilidade, t.Pontos, t.Patente, u.Ativo })
            .SingleOrDefaultAsync(cancellationToken);
        if (tecnico is null) return NotFound();
        var unidades = await (from v in _db.TecnicoUnidadesAtendimento.AsNoTracking()
                              join ua in _db.UnidadesAtendimento.AsNoTracking() on v.UnidadeAtendimentoId equals ua.Id
                              where v.TecnicoId == tecnicoId
                              orderby ua.Nome
                              select new { UnidadeId = ua.Id, Unidade = ua.Nome, v.Ativo, v.Responsavel })
            .ToListAsync(cancellationToken);
        return Ok(new { tecnico.TecnicoId, tecnico.UsuarioId, tecnico.Nome, tecnico.Email, tecnico.Telefone,
            tecnico.FotoUrl, tecnico.Disponibilidade, tecnico.MotivoIndisponibilidade, tecnico.Pontos,
            tecnico.Patente, tecnico.Ativo, Unidades = unidades });
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarTecnicoRequest request, CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        if (!SenhaValida(request.Senha))
            return BadRequest(new { message = "A senha deve ter ao menos 12 caracteres, com maiúscula, minúscula, número e símbolo." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Usuarios.AnyAsync(x => x.PrefeituraId == prefeituraId && x.Email.ToLower() == email, cancellationToken))
            return Conflict(new { message = "Já existe um usuário com este e-mail nesta prefeitura." });

        var unidadeIds = request.Unidades.Select(x => x.UnidadeId).ToArray();
        if (unidadeIds.Distinct().Count() != unidadeIds.Length)
            return BadRequest(new { message = "Não informe a mesma unidade mais de uma vez." });
        if (request.Unidades.Count(x => x.Responsavel) > 1)
            return BadRequest(new { message = "O técnico só pode ser responsável por uma unidade nesta operação." });
        if (unidadeIds.Length > 0 && await _db.UnidadesAtendimento.CountAsync(x => unidadeIds.Contains(x.Id)
                && x.PrefeituraId == prefeituraId && x.Ativa, cancellationToken) != unidadeIds.Length)
            return BadRequest(new { message = "Uma ou mais unidades não existem, estão inativas ou pertencem a outra prefeitura." });

        var unidadesComResponsavel = request.Unidades.Where(x => x.Responsavel).Select(x => x.UnidadeId).ToArray();
        if (unidadesComResponsavel.Length > 0 && await _db.TecnicoUnidadesAtendimento.AnyAsync(x =>
                unidadesComResponsavel.Contains(x.UnidadeAtendimentoId) && x.Ativo && x.Responsavel, cancellationToken))
            return Conflict(new { message = "Uma das unidades já possui técnico responsável. Transfira a responsabilidade pela operação de vínculo." });

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(), Nome = request.Nome.Trim(), Email = email,
            Telefone = request.Telefone.Trim(), PrefeituraId = prefeituraId,
            Papel = "Tecnico", Ativo = true
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Senha);
        var tecnico = new Tecnico
        {
            Id = Guid.NewGuid(), UsuarioId = usuario.Id, FotoUrl = request.FotoUrl,
            Disponibilidade = "Disponivel", DisponibilidadeAtualizadaEm = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        _db.Tecnicos.Add(tecnico);
        foreach (var unidade in request.Unidades)
            _db.TecnicoUnidadesAtendimento.Add(new TecnicoUnidadeAtendimento
            {
                Id = Guid.NewGuid(), TecnicoId = tecnico.Id, UnidadeAtendimentoId = unidade.UnidadeId,
                Ativo = true, Responsavel = unidade.Responsavel
            });

        await _unitOfWork.BeginTransaction(cancellationToken);
        try
        {
            await _unitOfWork.ConfirmTransaction(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RejectTransaction(cancellationToken);
            throw;
        }

        return Created($"/api/gestor/tecnicos/{tecnico.Id}", new
        {
            TecnicoId = tecnico.Id, UsuarioId = usuario.Id, usuario.Nome, usuario.Email,
            Unidades = request.Unidades
        });
    }

    [HttpPut("{tecnicoId:guid}/unidades")]
    public async Task<IActionResult> VincularUnidades(Guid tecnicoId, VincularUnidadesRequest request,
        CancellationToken cancellationToken)
    {
        if (!Tenant(out var prefeituraId)) return Forbid();
        var tecnico = await (from t in _db.Tecnicos
                             join u in _db.Usuarios on t.UsuarioId equals u.Id
                             where t.Id == tecnicoId && u.PrefeituraId == prefeituraId && u.Papel == "Tecnico"
                             select t).SingleOrDefaultAsync(cancellationToken);
        if (tecnico is null) return NotFound();

        var ids = request.Unidades.Select(x => x.UnidadeId).ToArray();
        if (ids.Distinct().Count() != ids.Length)
            return BadRequest(new { message = "Não informe a mesma unidade mais de uma vez." });
        if (request.Unidades.Count(x => x.Responsavel) > 1)
            return BadRequest(new { message = "Um técnico pode ser responsável por no máximo uma unidade nesta operação." });
        if (ids.Length > 0 && await _db.UnidadesAtendimento.CountAsync(x => ids.Contains(x.Id)
                && x.PrefeituraId == prefeituraId && x.Ativa, cancellationToken) != ids.Length)
            return BadRequest(new { message = "Uma ou mais unidades não existem, estão inativas ou pertencem a outra prefeitura." });

        var unidadesResponsaveis = request.Unidades.Where(x => x.Responsavel).Select(x => x.UnidadeId).ToArray();
        var conflitos = await _db.TecnicoUnidadesAtendimento.Where(x => x.TecnicoId != tecnicoId
                && x.Ativo && x.Responsavel && unidadesResponsaveis.Contains(x.UnidadeAtendimentoId))
            .ToListAsync(cancellationToken);
        foreach (var conflito in conflitos) conflito.Responsavel = false;

        var atuais = await _db.TecnicoUnidadesAtendimento.Where(x => x.TecnicoId == tecnicoId)
            .ToListAsync(cancellationToken);
        foreach (var atual in atuais)
        {
            var solicitado = request.Unidades.SingleOrDefault(x => x.UnidadeId == atual.UnidadeAtendimentoId);
            atual.Ativo = solicitado is not null;
            atual.Responsavel = solicitado?.Responsavel ?? false;
        }
        foreach (var unidade in request.Unidades.Where(x => atuais.All(a => a.UnidadeAtendimentoId != x.UnidadeId)))
            _db.TecnicoUnidadesAtendimento.Add(new TecnicoUnidadeAtendimento
            {
                Id = Guid.NewGuid(), TecnicoId = tecnicoId, UnidadeAtendimentoId = unidade.UnidadeId,
                Ativo = true, Responsavel = unidade.Responsavel
            });

        await _unitOfWork.BeginTransaction(cancellationToken);
        try
        {
            await _unitOfWork.ConfirmTransaction(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RejectTransaction(cancellationToken);
            throw;
        }
        return NoContent();
    }

    private bool Tenant(out Guid prefeituraId)
        => Guid.TryParse(User.FindFirstValue("prefeitura_id"), out prefeituraId);

    private static bool SenhaValida(string senha)
        => senha.Length >= 12 && senha.Any(char.IsUpper) && senha.Any(char.IsLower)
            && senha.Any(char.IsDigit) && senha.Any(c => !char.IsLetterOrDigit(c));
}

public sealed record CriarTecnicoRequest(
    [Required, StringLength(180, MinimumLength = 2)] string Nome,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(30)] string Telefone,
    [Required, StringLength(200)] string Senha,
    [StringLength(1000)] string? FotoUrl,
    [Required] List<UnidadeTecnicoRequest> Unidades);

public sealed record VincularUnidadesRequest([Required] List<UnidadeTecnicoRequest> Unidades);
public sealed record UnidadeTecnicoRequest([Required] Guid UnidadeId, bool Responsavel = false);

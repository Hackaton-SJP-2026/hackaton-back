using FluentValidation;
using MapsterMapper;
using Hackaton.Application.Exceptions;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Domain.Interface;

namespace Hackaton.Application.Services
{
    public class AppService<TEntity, TDto> : IAppService<TEntity, TDto> where TEntity : Entity
    {
        protected readonly IDomainService<TEntity> _domainService;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly IEnumerable<IValidator<TDto>> _validators;

        public AppService(
            IDomainService<TEntity> domainService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEnumerable<IValidator<TDto>> validators)
        {
            _domainService = domainService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validators = validators;
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _domainService.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<TDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _domainService.GetByIdAsync(id, cancellationToken);
            return entity is null ? default : _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto> CreateAsync(TDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            await ValidarAsync(dto, cancellationToken);

            var entity = _mapper.Map<TEntity>(dto);
            entity.Id = Guid.NewGuid();

            _unitOfWork.Begin();
            _domainService.Add(entity);
            await _unitOfWork.Confirm(cancellationToken);

            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto> UpdateAsync(Guid id, TDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            await ValidarAsync(dto, cancellationToken);

            var existente = await _domainService.GetByIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(typeof(TEntity).Name, id);

            _mapper.Map(dto, existente);
            existente.Id = id;

            _unitOfWork.Begin();
            _domainService.Update(existente);
            await _unitOfWork.Confirm(cancellationToken);

            return _mapper.Map<TDto>(existente);
        }

        public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _domainService.GetByIdAsync(id, cancellationToken)
                ?? throw new EntityNotFoundException(typeof(TEntity).Name, id);

            _unitOfWork.Begin();
            _domainService.Remove(entity);
            await _unitOfWork.Confirm(cancellationToken);
        }

        protected virtual async Task ValidarAsync(TDto dto, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return;

            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(dto, cancellationToken))))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .Select(f => f.ErrorMessage)
                .ToList();

            if (failures.Count > 0)
                throw new EntityValidationException(failures);
        }
    }
}

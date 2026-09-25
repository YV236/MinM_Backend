namespace MinM_API.Validators
{
    public interface IDtoValidator
    {
        Type DtoType { get; }
        Task ValidateAsync(object dto, CancellationToken cancellationToken);
    }

    public abstract class DtoValidator<TDto> : IDtoValidator
    {
        public Type DtoType => typeof(TDto);

        public Task ValidateAsync(object dto, CancellationToken cancellationToken) =>
            ValidateAsync((TDto)dto, cancellationToken);

        protected abstract Task ValidateAsync(TDto dto, CancellationToken cancellationToken);
    }
}

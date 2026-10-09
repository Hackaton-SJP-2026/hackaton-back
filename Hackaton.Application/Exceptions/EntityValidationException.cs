namespace Hackaton.Application.Exceptions
{
    public sealed class EntityValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public EntityValidationException(IEnumerable<string> errors)
            : base(string.Join(" | ", errors))
        {
            Errors = errors.ToList();
        }

        public EntityValidationException(string error)
            : this(new[] { error })
        {
        }
    }
}

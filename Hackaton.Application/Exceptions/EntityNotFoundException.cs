namespace Hackaton.Application.Exceptions
{
    public sealed class EntityNotFoundException : Exception
    {
        public string EntityName { get; }
        public object Key { get; }

        public EntityNotFoundException(string entityName, object key)
            : base($"{entityName} {key} não encontrado(a).")
        {
            EntityName = entityName;
            Key = key;
        }
    }
}

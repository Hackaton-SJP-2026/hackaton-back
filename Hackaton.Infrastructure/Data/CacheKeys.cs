namespace Hackaton.Infrastructure.Data
{
    internal static class CacheKeys
    {
        public static string All(string entityTypeName) => $"Repository:{entityTypeName}:all";

        public static string All<T>() => All(typeof(T).Name);
    }
}

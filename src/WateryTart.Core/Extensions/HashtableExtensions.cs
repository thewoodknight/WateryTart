using System.Collections;

namespace WateryTart.Core.Extensions
{
    public static class HashtableExtensions
    {
        public static object? TryGet<T>(this Hashtable table, string Key)
        {
            if (table == null || string.IsNullOrEmpty(Key))
                return null;

            if (table.ContainsKey(Key))
                return (T)table[Key]!;

            return null;
        }
    }
}

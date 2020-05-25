using System.Collections.Generic;
using System.Collections.Immutable;

namespace Darchatty.WebApp.Model
{
    public static class ImmutableExtensions
    {
        /// <summary>
        /// Given a dictionary, if it is already an ImmutableDictionary, returns source.  Otherwise, returns the result of calling ToImmutableDictionary on source.
        /// </summary>
        public static ImmutableDictionary<TKey, TValue> AsImmutableDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)
        {
            if (source is ImmutableDictionary<TKey, TValue> already)
            {
                return already;
            }

            return source.ToImmutableDictionary(x => x.Key, x => x.Value);
        }
    }
}

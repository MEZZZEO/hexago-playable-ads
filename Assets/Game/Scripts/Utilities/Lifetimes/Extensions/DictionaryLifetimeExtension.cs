using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class DictionaryLifetimeExtension
    {
        public static void AddLifetimed<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            Lifetime lifetime,
            TKey key,
            TValue value)
        {
            dictionary.AddLifetimed(lifetime, new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}

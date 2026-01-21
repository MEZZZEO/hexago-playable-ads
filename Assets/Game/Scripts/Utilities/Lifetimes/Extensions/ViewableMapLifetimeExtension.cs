using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class ViewableMapLifetimeExtension
    {
        public static void AddLifetimed<TKey, TValue>(this IViewableMap<TKey, TValue> map, Lifetime lifetime, TKey key, TValue value)
        {
            map.Add(new KeyValuePair<TKey, TValue>(key, value));
            lifetime.OnTermination(() => map.Remove(key));
        }
    }
}



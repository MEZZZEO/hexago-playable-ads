using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class ViewableSetLifetimeExtension
    {
        public static void AddLifetimed<T>(this IViewableSet<T> set, Lifetime lifetime, T value)
        {
            set.Add(value);
            lifetime.OnTermination(() => set.Remove(value));
        }
    }
}



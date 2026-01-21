using System;
using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class DisposableLifetimeExtensions
    {
        public static void AddTo(this IDisposable disposable, Lifetime lifetime)
        {
            lifetime.OnTermination(disposable.Dispose);
        }
    }
}

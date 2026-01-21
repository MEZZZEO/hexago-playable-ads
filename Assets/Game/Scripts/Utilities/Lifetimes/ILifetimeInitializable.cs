using JetBrains.Lifetimes;

namespace Game.Utilities.Lifetimes
{
    public interface ILifetimeInitializable
    {
        void Initialize(Lifetime lifetime);
    }
}

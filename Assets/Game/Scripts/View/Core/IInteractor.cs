using JetBrains.Lifetimes;

namespace Game.Scripts.View.Core
{
    public interface IInteractor { }

    public interface IInteractor<TProtocol> : IInteractor where TProtocol : IProtocol
    {
        TProtocol Get(Lifetime lifetime);
    }
}

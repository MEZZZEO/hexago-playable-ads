using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using Zenject;

namespace Game.Scripts.View.Core
{
    public abstract class MonoPresenter : MonoBehaviour
    {
        protected IProtocolDispatcher ProtocolDispatcher;
        private LifetimeDefinition _definition;

        [Inject]
        private void SetDependencies(IProtocolDispatcher protocolDispatcher)
        {
            ProtocolDispatcher = protocolDispatcher;
        }

        protected virtual void OnEnable()
        {
            _definition = new();
            Observable.Return(Unit.Default)
                .Subscribe(_ => Setup(_definition.Lifetime))
                .AddTo(_definition.Lifetime);
        }

        private void OnDisable()
        {
            _definition.Terminate();
        }

        protected abstract void Setup(Lifetime lifetime);
    }
}

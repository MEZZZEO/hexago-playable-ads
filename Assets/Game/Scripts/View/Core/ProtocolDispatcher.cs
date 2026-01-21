using System;
using System.Collections.Generic;
using System.Linq;
using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Game.Scripts.View.Core
{
    public interface IProtocolDispatcher
    {
        TProtocol Get<TProtocol>(Lifetime lifetime) where TProtocol : IProtocol;
    }

    public class ProtocolDispatcher : IProtocolDispatcher
    {
        private readonly Dictionary<Type, IInteractor> _interactors = new();
        private readonly Dictionary<Type, ProtocolInfo> _activeProtocols = new();

        private readonly IProtocolDispatcher _parentDispatcher;

        public ProtocolDispatcher([InjectLocal] List<IInteractor> interactors, [InjectParent(true)] IProtocolDispatcher parentDispatcher)
        {
            _parentDispatcher = parentDispatcher;
            FillInteractorsDictionary(interactors);
        }

        private void FillInteractorsDictionary(List<IInteractor> interactors)
        {
            foreach (var interactor in interactors)
            {
                try
                {
                    var protocol = interactor.GetType()
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IInteractor<>))
                        .SelectMany(i => i.GetGenericArguments())
                        .Single();

                    _interactors[protocol] = interactor;
                }
                catch (Exception)
                {
                    Debug.Log(
                        $"Interactor {interactor.GetType()} must implement one and only one {typeof(IInteractor<>)} interface");
                }
            }
        }

        public TProtocol Get<TProtocol>(Lifetime lifetime) where TProtocol : IProtocol
        {
            if (TryGetOrActivateProtocol<TProtocol>(out var protocolInfo, lifetime))
            {
                return (TProtocol)protocolInfo.Protocol;
            }

            if (_parentDispatcher != null)
            {
                return _parentDispatcher.Get<TProtocol>(lifetime);
            }

            throw new InvalidOperationException(
                $"Protocol {typeof(TProtocol)} cannot be resolved by {nameof(ProtocolDispatcher)}");
        }

        private bool TryGetOrActivateProtocol<TProtocol>(out ProtocolInfo protocolInfo, Lifetime lifetime) where TProtocol : IProtocol
        {
            var protocolType = typeof(TProtocol);
            if (_activeProtocols.TryGetValue(protocolType, out protocolInfo))
            {
                protocolInfo.IncreaseReferenceCount(lifetime);
                return true;
            }

            if (!_interactors.TryGetValue(protocolType, out var interactor))
            {
                return false;
            }

            var lifetimeDefinition = new LifetimeDefinition();
            protocolInfo = new ProtocolInfo
            {
                LifetimeDefinition = lifetimeDefinition,
                Protocol = ((IInteractor<TProtocol>)interactor).Get(lifetimeDefinition.Lifetime)
            };
            _activeProtocols.AddLifetimed(lifetimeDefinition.Lifetime, protocolType, protocolInfo);
            
            protocolInfo.IncreaseReferenceCount(lifetime);
            return true;
        }
        
        private class ProtocolInfo
        {
            public IProtocol Protocol;
            public LifetimeDefinition LifetimeDefinition;

            private int _referenceCount;

            public void IncreaseReferenceCount(Lifetime lifetime)
            {
                _referenceCount++;
                lifetime.OnTermination(DecreaseReferenceCount);
            }

            private void DecreaseReferenceCount()
            {
                _referenceCount--;
                if (_referenceCount == 0)
                {
                    LifetimeDefinition.Terminate();
                }
            }
        }
    }
}

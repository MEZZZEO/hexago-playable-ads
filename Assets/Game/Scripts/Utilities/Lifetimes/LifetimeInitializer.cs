using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;
using ModestTree;
using Zenject;

namespace Game.Utilities.Lifetimes
{
    public class LifetimeInitializer
    {
        private readonly List<ILifetimeInitializable> _initializables;
        private bool _hasInitialized;

        public LifetimeInitializer([InjectLocal] List<ILifetimeInitializable> initializables)
        {
            _initializables = initializables; 
        }
        
        public void Initialize(Lifetime lifetime)
        {
            Assert.That(!_hasInitialized, "LifetimeInitializer can only be initialized once");
            _hasInitialized = true;

#if UNITY_EDITOR
            foreach (var initializable in _initializables.GetDuplicates())
            {
                Assert.That(false, "Found duplicate ILifetimeInitializable with type '{0}'".Fmt(initializable.GetType()));
            }
#endif

            foreach (var initializable in _initializables)
            {
                try
                {
#if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
#if UNITY_EDITOR
                    using (ProfileBlock.Start("{0}.Initialize()", initializable.GetType()))
#endif
                    {
                        initializable.Initialize(lifetime);
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(
                        e, "Error occurred while initializing ILifetimeInitializable with type '{0}'", initializable.GetType());
                }
            }
        }
    }
}

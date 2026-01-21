using Game.Utilities.Lifetimes.Extensions;
using UnityEngine;
using Zenject;

namespace Game.Utilities.Lifetimes
{
    public class LifetimeStarter : MonoBehaviour, IInitializable
    {
        private LifetimeInitializer _lifetimeInitializer;

        [Inject]
        public void SetDependencies(LifetimeInitializer lifetimeInitializer)
        {
            _lifetimeInitializer = lifetimeInitializer;
        }
        
        public void Initialize()
        {
            _lifetimeInitializer.Initialize(gameObject.GetLifetime());
        }
    }
}

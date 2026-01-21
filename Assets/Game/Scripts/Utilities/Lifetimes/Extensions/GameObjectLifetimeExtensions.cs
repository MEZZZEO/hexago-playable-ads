using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class GameObjectLifetimeExtensions
    {
        public static Lifetime GetLifetime(this GameObject gameObject)
        {
            return gameObject.AddOrGetComponent<LifetimeComponent>().Lifetime;
        }

        private sealed class LifetimeComponent : MonoBehaviour
        {
            private readonly LifetimeDefinition _definition = new();
            public Lifetime Lifetime => _definition.Lifetime;

            private void OnDestroy()
            {
                _definition.Terminate();
            }
        }
    }
}

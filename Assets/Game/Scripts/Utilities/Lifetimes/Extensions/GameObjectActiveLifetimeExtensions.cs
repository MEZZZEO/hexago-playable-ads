using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class GameObjectActiveLifetimeExtensions
    {
        public static Lifetime GetActiveLifetime(this GameObject gameObject)
        {
            var component = gameObject.AddOrGetComponent<ActiveLifetimeComponent>();
            return component.Lifetime;
        }
        
        public static ActiveLifetimeComponent GetActiveLifetimeComponent(this GameObject gameObject)
        {
            return gameObject.AddOrGetComponent<ActiveLifetimeComponent>();
        }

        [DefaultExecutionOrder(-100)]
        public class ActiveLifetimeComponent : MonoBehaviour
        {
            private readonly ViewableProperty<bool> _isActive = new(false);
            private LifetimeDefinition _definition;
            
            public IReadonlyProperty<bool> IsActive => _isActive;
            public Lifetime Lifetime => _definition.Lifetime;

            private void OnEnable()
            {
                _definition = gameObject.GetLifetime().CreateNested();
                _isActive.Value = true;
            }

            private void OnDisable()
            {
                _definition.Terminate();
                _isActive.Value = false;
            }
        }
    }
}

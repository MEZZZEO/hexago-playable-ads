using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class SetActiveLifetimedExtension
    {
        public static void SetActiveLifetimed(this GameObject gameObject, Lifetime lifetime)
        {
            gameObject.SetActive(true);
            lifetime.OnTermination(() => gameObject.SetActive(false));
        }
        
        public static void SetActiveWhileTrue(this GameObject gameObject, Lifetime lifetime, IReadonlyProperty<bool> property)
        {
            var gameObjectLifetime = gameObject.GetLifetime();
            var intersectedLifetime = gameObjectLifetime.Intersect(lifetime);
            
            property.Advise(intersectedLifetime, gameObject.SetActive);
        }

        public static void SetActiveWhileFalse(this GameObject gameObject, Lifetime lifetime, IReadonlyProperty<bool> property)
        {
            var gameObjectLifetime = gameObject.GetLifetime();
            var intersectedLifetime = gameObjectLifetime.Intersect(lifetime);
            
            property.Advise(intersectedLifetime, value => gameObject.SetActive(!value));
        }
    }
}

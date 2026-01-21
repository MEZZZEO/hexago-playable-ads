using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Game.Utilities.Lifetimes.Extensions
{
    public static class LifetimedInstatiateExtensions
    {
        public static GameObject Instantiate(this GameObject gameObject, Lifetime lifetime, Transform parent = null)
        {
            return lifetime.Bracket(
                opening: () => Object.Instantiate(gameObject, parent),
                closing: Object.Destroy
            );
        }

        public static GameObject InstantiatePrefab(this IInstantiator instantiator, Lifetime lifetime, Object prefab, Transform parent = null)
        {
            return lifetime.Bracket(
                opening: () => instantiator.InstantiatePrefab(prefab, parent),
                closing: Object.Destroy
            );
        }

        public static GameObject InstantiatePrefab(
            this IInstantiator instantiator, Lifetime lifetime, Object prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return lifetime.Bracket(
                opening: () => instantiator.InstantiatePrefab(prefab, position, rotation, parent),
                closing: Object.Destroy
            );
        }
        
        public static T InstantiatePrefabForComponent<T>(this IInstantiator instantiator, Lifetime lifetime, T prefab, Transform parent = null)
            where T : Object
        {
            return lifetime.Bracket(
                opening: () => instantiator.InstantiatePrefabForComponent<T>(prefab, parent),
                closing: Object.Destroy);
        }
    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Utilities.Addressables
{
    public class LocalPool : ILocalPool
    {
        private readonly Lifetime _appLifetime;
        private readonly DiContainer _diContainer;
        private readonly Transform _container;
        private readonly Queue<Item> _pool = new();
        private Transform _itemPlace;
        private int _limit;
        public ComponentReference Reference { get; }

        public LocalPool(Lifetime appLifetime, DiContainer diContainer, ComponentReference reference, Transform container)
        {
            _appLifetime = appLifetime;
            _diContainer = diContainer;
            Reference = reference;
            _container = container;
        }

        public async UniTask Warmup(Lifetime lifetime, int count, int limit)
        {
            _limit += limit;

            var prefab = Reference.GetPrefab();
            if (prefab == null)
            {
                Debug.LogError($"[LocalPool] Prefab not set for {Reference.AssetGUID}");
                return;
            }
            
            prefab.SetActive(false);
            _itemPlace = _container.Find(prefab.name);
            if (_itemPlace == null)
            {
                _itemPlace = new GameObject(prefab.name).transform;
                _itemPlace.SetParent(_container);
            }

            for (var i = 0; i < count; i++)
            {
                InstantiatePrefab(prefab);
            }

            lifetime.OnTermination(() => Release(limit));
            
            await UniTask.CompletedTask;
        }

        public async UniTask<Poolable> Rent(Lifetime lifetime)
        {
            lifetime.ThrowIfNotAlive();

            Item pooled;
            while (!_pool.TryDequeue(out pooled) || pooled.Component == null || !pooled.Component)
            {
                if (_pool.Count == 0)
                {
                    await InstantiateAsync();
                }
            }

            var item = pooled.Component;
            item.transform.SetParent(null);
            //ngo Р·Р°С‡РµРј-С‚Рѕ РїРµСЂРµРЅРѕСЃРёС‚ РЅР° Р°РєС‚РёРІРЅСѓСЋ СЃС†РµРЅСѓ (РґР°Р¶Рµ РїСЂРё РІС‹РєР»СЋС‡РµРЅРЅРѕРј Synchronize Active Scene), РїРѕСЌС‚РѕРјСѓ РјРѕР¶РµС‚ Р·Р°СЃРїР°РІРЅРёС‚СЊ РЅР° РІСЂРµРјРµРЅРЅРѕР№ СЃС†РµРЅРµ Рё Р·Р°С‚РµРј РІС‹РіСЂСѓР·РёС‚СЊ РѕР±СЉРµРєС‚
            Object.DontDestroyOnLoad(item);

            lifetime.OnTermination(() => Return(pooled));

            item.gameObject.SetActive(true);
            item.OnRent(lifetime).Forget();
            return item;
        }

        private async UniTask InstantiateAsync()
        {
            try
            {
                var prefab = Reference.GetPrefab();
                
                if (prefab == null)
                {
                    Debug.LogError($"[LocalPool] Failed to load prefab for {Reference.AssetGUID}");
                    return;
                }
                
                prefab.SetActive(false);
                InstantiatePrefab(prefab);
                
                await UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void InstantiatePrefab(GameObject prefab)
        {
            var instanceDefinition = _appLifetime.CreateNested();
            var instanceLifetime = instanceDefinition.Lifetime;
            var instance = _diContainer.InstantiatePrefab(prefab, _itemPlace);
            Poolable component = instance.GetComponent<Poolable>();
            instance.SetActive(false);

            instanceLifetime.OnTermination(() =>
            {
#if UNITY_EDITOR
                if (!PlayModeUtility.IsPlaying)
                    return;
#endif
                Object.Destroy(component.gameObject);
            });

            _pool.Enqueue(new Item(instanceDefinition, component));
        }

        private void Release(int limit)
        {
            _limit -= limit;
            var countExtraItems = _pool.Count - _limit;
            if (countExtraItems > 0)
            {
                for (var i = 0; i < countExtraItems; i++)
                {
                    var item = _pool.Dequeue();
                    item.Definition.Terminate();
                }
            }

            if (_limit == 0)
            {
#if UNITY_EDITOR
                if (!PlayModeUtility.IsPlaying)
                    return;
#endif
                Object.Destroy(_itemPlace.gameObject);
            }
        }

        private void Return(Item pooled) => ReturnAsync(pooled).Forget();

        private async UniTaskVoid ReturnAsync(Item pooled)
        {
            try
            {
                if (!await DeactivateObject(pooled))
                {
                    return;
                }

                await UniTask.Yield();
                
                if (pooled.Component == null || !pooled.Component)
                    return;
                
                pooled.Component.transform.SetParent(_itemPlace, false);
                _pool.Enqueue(pooled);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static async UniTask<bool> DeactivateObject(Item pooled)
        {
#if UNITY_EDITOR
            if (!PlayModeUtility.IsPlaying)
                return true;
#endif
            if (pooled == null)
            {
                Debug.LogError("Pooled item is null");
                return false;
            }

            var component = pooled.Component;
            if (!component)
            {
                Debug.LogError("Pooled item is empty");
                return false;
            }

            if (!component.gameObject)
            {
                Debug.LogError("Pooled item game object is null");
                return false;
            }

            if (pooled.Definition.Lifetime.IsNotAlive)
            {
                Debug.LogWarning($"Pooled item lifetime is terminated {component.name} {component.GetInstanceID()}");
                return false;
            }

            await component.OnReturn();

            if (!component)
            {
                Debug.LogError("Pooled item is empty after return");
                return false;
            }

            if (!component.gameObject)
            {
                Debug.LogError("Pooled item game object is null after return");
                return false;
            }

            component.gameObject.SetActive(false);
            return true;
        }


        private class Item
        {
            public readonly Poolable Component;
            public readonly LifetimeDefinition Definition;

            public Item(LifetimeDefinition definition, Poolable component)
            {
                Component = component;
                Definition = definition;
            }
        }
    }
}

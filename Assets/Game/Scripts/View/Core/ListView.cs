using System;
using Cysharp.Threading.Tasks;
using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Game.Scripts.View.Core
{
    public interface IListView<T>
    {
        void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<GameObject>> prefabFactory = null);
    }

    public interface IListViewItem<in T>
    {
        void Bind(Lifetime lifetime, T source);
    }

    public abstract class ListView<T> : ListViewBase<T>, IListView<T>
    {
        private GameObject _prefab;
        private Func<Lifetime, T, UniTask<GameObject>> _prefabFactory;

        [Inject] private IInstantiator _instantiator;

        public new void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<GameObject>> prefabFactory = null)
        {
            prefabFactory ??= (_, _) => UniTask.FromResult(_prefab);
            _prefabFactory = prefabFactory;
            if (transform.childCount > 0)
            {
                _prefab = transform.GetChild(0).gameObject;
                _prefab.SetActive(false);
            }

            base.Bind(lifetime, source, CreateInstance);
        }

        private async UniTask<GameObject> CreateInstance(Lifetime lifetime, T source)
        {
            var prefab = await _prefabFactory.Invoke(lifetime, source);
            GameObject instance = _instantiator.InstantiatePrefab(lifetime, prefab);
            instance.SetActive(true);
            return instance;
        }
    }
}

using System;
using Cysharp.Threading.Tasks;
using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Game.Scripts.View.Core
{
    public abstract class ListViewBase<T> : MonoBehaviour
    {
        private Func<Lifetime, T, UniTask<GameObject>> _instanceFactory;
        
        public void Bind(Lifetime lifetime, IViewableList<T> source, Func<Lifetime, T, UniTask<GameObject>> instanceFactory)
        {
            _instanceFactory = instanceFactory;
            gameObject.GetActiveLifetimeComponent().IsActive
                .WhenTrue(lifetime, activeLifetime => { source.View(activeLifetime, OnNewItem); });
        }

        private void OnNewItem(Lifetime lifetime, int index, T source)
        {
            if (lifetime.IsNotAlive)
                return;
            InstantiatePrefab(lifetime, index, source).Forget();
        }

        private async UniTask InstantiatePrefab(Lifetime lifetime, int index, T source)
        {
            var item = await _instanceFactory.Invoke(lifetime, source);
            if (lifetime.IsNotAlive)
                return;
            item.transform.SetParent(transform, false);
            item.transform.SetSiblingIndex(index);
            item
                .GetComponent<IListViewItem<T>>()
                .Bind(lifetime, source);
        }
    }
}

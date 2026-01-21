using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.View.Core;
using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.View.Core
{
    public abstract class ToggleListView<T> : ListViewBase<T>, IListView<T>
    {
        [SerializeField] private ToggleGroup _toggleGroup;
        
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
            var instance = _instantiator.InstantiatePrefab(lifetime, prefab);
            instance.SetActive(true);
            
            // РђРІС‚РѕРјР°С‚РёС‡РµСЃРєРё РґРѕР±Р°РІР»СЏРµРј Toggle РІ ToggleGroup
            if (_toggleGroup != null)
            {
                var toggle = instance.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.group = _toggleGroup;
                }
            }
            
            return instance;
        }
    }
}



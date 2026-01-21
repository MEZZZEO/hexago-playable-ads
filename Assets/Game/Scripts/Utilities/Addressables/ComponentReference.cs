using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Utilities.Addressables
{
    [Serializable]
    public class ComponentReference
    {
        [SerializeField]
        private GameObject _prefab;
        
        [SerializeField]
        private string _guid;

        public string AssetGUID
        {
            get
            {
                // Р•СЃР»Рё GUID РїСѓСЃС‚, РіРµРЅРµСЂРёСЂСѓРµРј РµРіРѕ РЅР° РѕСЃРЅРѕРІРµ РёРјРµРЅРё prefab (РїСЂРѕСЃС‚РѕР№ С„Р°Р»Р»Р±СЌРє)
                if (string.IsNullOrEmpty(_guid) && _prefab != null)
                {
                    _guid = _prefab.name;
                }
                return _guid;
            }
        }

        public ComponentReference(GameObject prefab, string guid = "")
        {
            _prefab = prefab;
            _guid = guid;
        }

        public ComponentReference()
        {
        }

        public GameObject GetPrefab()
        {
            return _prefab;
        }
        
        public void SetPrefab(GameObject prefab, string guid = "")
        {
            _prefab = prefab;
            _guid = guid;
        }
    }

    public abstract class ComponentReference<TComponent> : ComponentReference where TComponent : Component
    {
        public ComponentReference(GameObject prefab, string guid = "") : base(prefab, guid)
        {
        }

        public ComponentReference() : base()
        {
        }

        public bool ValidateAsset(Object obj)
        {
            var go = obj as GameObject;
            return go != null && go.GetComponent<TComponent>() != null;
        }

        public bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go != null && go.GetComponent<TComponent>() != null;
#else
            return false;
#endif
        }
    }
}




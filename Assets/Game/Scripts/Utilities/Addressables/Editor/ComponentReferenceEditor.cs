using UnityEditor;
using UnityEngine;

namespace Game.Utilities.Addressables.Editor
{
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ComponentReference), useForChildren: true)]
    public class ComponentReferencePropertyDrawer : PropertyDrawer
    {
        private const float Spacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Р РёСЃСѓРµРј РѕСЃРЅРѕРІРЅРѕРµ РїРѕР»Рµ РґР»СЏ СЃРІРµСЂРЅСѓС‚РѕРіРѕ РїСЂРµРґСЃС‚Р°РІР»РµРЅРёСЏ
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var prefabProp = property.FindPropertyRelative("_prefab");
            var guidProp = property.FindPropertyRelative("_guid");

            // Р РёСЃСѓРµРј РїРѕР»Рµ prefab
            var prefabRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var newPrefab = EditorGUI.ObjectField(prefabRect, prefabProp.objectReferenceValue, typeof(GameObject), false);

            // Р•СЃР»Рё prefab Р±С‹Р» РёР·РјРµРЅРµРЅ, Р°РІС‚РѕРјР°С‚РёС‡РµСЃРєРё Р·Р°РїРѕР»РЅСЏРµРј GUID
            if (newPrefab != prefabProp.objectReferenceValue)
            {
                prefabProp.objectReferenceValue = newPrefab;
                
                if (newPrefab != null)
                {
                    var prefabAsset = newPrefab as GameObject;
                    var assetPath = AssetDatabase.GetAssetPath(prefabAsset);
                    
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        // РСЃРїРѕР»СЊР·СѓРµРј СЂРµР°Р»СЊРЅС‹Р№ GUID РёР· AssetDatabase
                        guidProp.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
                    }
                    else
                    {
                        // Р•СЃР»Рё СЌС‚Рѕ Runtime РѕР±СЉРµРєС‚, РёСЃРїРѕР»СЊР·СѓРµРј РёРјСЏ
                        guidProp.stringValue = prefabAsset.name;
                    }
                }
                else
                {
                    // РћС‡РёС‰Р°РµРј GUID РµСЃР»Рё prefab СѓРґР°Р»РµРЅ
                    guidProp.stringValue = "";
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
    #endif
}


#if UNITY_EDITOR
using Game.Gameplay.Configs;
using UnityEditor;
using UnityEngine;

namespace Game.Gameplay.Editor
{
    [CustomEditor(typeof(GameplayConfig))]
    public class GameplayConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Auto-Find Prefabs in Project"))
            {
                AutoFindPrefabs();
            }
        }

        private void AutoFindPrefabs()
        {
            var config = (GameplayConfig)target;
            var so = new SerializedObject(config);
            
            // Поиск префабов по имени
            TryAssignPrefab(so, "_hexPiecePrefab", "HexPiece");
            TryAssignPrefab(so, "_hexStackPrefab", "HexStack");
            TryAssignPrefab(so, "_gridCellPrefab", "GridCell");
            TryAssignPrefab(so, "_cellBackgroundPrefab", "CellBackground");
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            
            Debug.Log("[GameplayConfigEditor] Prefabs auto-assigned!");
        }

        private void TryAssignPrefab(SerializedObject so, string propertyName, string prefabName)
        {
            var guids = AssetDatabase.FindAssets($"t:Prefab {prefabName}");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (go != null && go.name == prefabName)
                {
                    var prop = so.FindProperty(propertyName);
                    if (prop != null)
                    {
                        var prefabProp = prop.FindPropertyRelative("_prefab");
                        var guidProp = prop.FindPropertyRelative("_guid");
                        
                        if (prefabProp != null)
                        {
                            prefabProp.objectReferenceValue = go;
                        }
                        if (guidProp != null)
                        {
                            guidProp.stringValue = guid;
                        }
                        
                        Debug.Log($"[GameplayConfigEditor] Found and assigned: {prefabName} at {path}");
                    }
                    return;
                }
            }
            
            Debug.LogWarning($"[GameplayConfigEditor] Could not find prefab: {prefabName}");
        }
    }
}
#endif


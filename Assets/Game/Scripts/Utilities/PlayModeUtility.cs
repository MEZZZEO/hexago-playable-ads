using UnityEditor;
using UnityEngine;

namespace Game.Utilities
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class PlayModeUtility
    {
        public static bool IsPlaying { get; private set; }

#if UNITY_EDITOR
        static PlayModeUtility()
        {
            IsPlaying = Application.isPlaying;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            IsPlaying = obj == PlayModeStateChange.EnteredPlayMode;
        }
#else
        static PlayModeUtility()
        {
            IsPlaying = true;
        }
#endif
    }
}

#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

namespace TaleUtil
{
    public partial class Editor
    {
        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;

            void OnGUI()
            {
                EditorGUILayout.LabelField("Enter the transition name:");
                name = EditorGUILayout.TextField(name);

                EditorGUILayout.Space(10);

                if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return)
                {
                    CreateTaleTransition(FindTaleMaster(), name);
                    Close();
                }
            }
        }

        public class CreateSplashSceneDialog : EditorWindow
        {
            public new string name;
            public Sprite logo;
            public AudioClip sound;

            void OnGUI()
            {
                EditorGUILayout.LabelField("Enter the splash scene name:");
                name = EditorGUILayout.TextField(name);

                EditorGUILayout.Space(20);

                logo = (Sprite) EditorGUILayout.ObjectField("Splash logo", logo, typeof(Sprite), false);

                EditorGUILayout.Space(10);

                sound = (AudioClip)EditorGUILayout.ObjectField("Splash sound", sound, typeof(AudioClip), false);

                EditorGUILayout.Space(10);

                if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return)
                {
                    CreateSplashScene(name, logo, sound);
                    Close();
                }
            }
        }
    }
}
#endif
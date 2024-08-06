#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleUtil
{
    public partial class Editor
    {
        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                EditorGUILayout.LabelField("Enter the transition name:");
                name = EditorGUILayout.TextField(name);

                EditorGUILayout.Space(10);

                if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return)
                {
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(TALE_PREFAB_PATH))
                    {
                        CreateTaleTransition(scope.prefabContentsRoot, name);
                    }

                    Close();
                }
            }
        }

        public class CreateSplashSceneDialog : EditorWindow
        {
            public new string name;
            public Sprite logo;
            public List<AudioClip> soundVariants;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                EditorGUILayout.LabelField("Enter the splash scene name:");
                name = EditorGUILayout.TextField(name);

                EditorGUILayout.Space(20);

                logo = (Sprite) EditorGUILayout.ObjectField("Splash logo", logo, typeof(Sprite), false);

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Splash Sound Variants", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                if (soundVariants != null)
                {
                    for (int i = 0; i < soundVariants.Count; i++)
                    {
                        soundVariants[i] = (AudioClip) EditorGUILayout.ObjectField("Sound " + i, soundVariants[i], typeof(AudioClip), false);
                    }
                }

                EditorGUI.indentLevel--;

                if (GUILayout.Button("Add Splash Sound"))
                {
                    if (soundVariants == null)
                    {
                        soundVariants = new List<AudioClip>();
                    }

                    soundVariants.Add(null);
                }

                EditorGUILayout.Space(5);

                if (GUILayout.Button("OK") || Event.current.keyCode == KeyCode.Return)
                {
                    CreateSplashScene(name, logo, soundVariants);
                    Close();
                }
            }
        }
    }
}
#endif
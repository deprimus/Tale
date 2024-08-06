#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TaleUtil
{
    public partial class Editor
    {
        [MenuItem("Tale/Compile Story", priority = 3)]
        static void CompileStory()
        {
            CompileStoryDialog dialog = EditorWindow.GetWindow<CompileStoryDialog>();
            dialog.titleContent = new GUIContent("Tale - Compile Story");
            dialog.minSize = new Vector2(400f, 210f);
            dialog.maxSize = dialog.minSize;
            dialog.ShowPopup();
        }

        public class CompileStoryDialog : EditorWindow
        {
            public string file = null;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                EditorGUILayout.LabelField(file ?? "<Select story file>", new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false
                });

                if (GUILayout.Button("Browse"))
                {
                    string path = EditorUtility.OpenFilePanelWithFilters("Select Story File", System.IO.Path.Combine(Application.dataPath, "Scripts"), new string[] { "Tale story files", "md" });
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        file = path;
                    }
                }

                EditorGUILayout.Space(5);

                if (!string.IsNullOrEmpty(file))
                {
                    if (GUILayout.Button("Compile Selected") || Event.current.keyCode == KeyCode.Return)
                    {
                        try
                        {
                            if (!StoryCompiler.Compile(file, System.IO.Path.Combine(Application.dataPath, "Scripts")))
                            {
                                EditorUtility.DisplayDialog("Compilation failed", "An error occurred during compilation. Please check the logs.", "Ok");
                            }
                        }
                        catch(Exception ex)
                        {
                            Log.Error("Compiler", ex.ToString());
                            EditorUtility.DisplayDialog("Compilation failed", "An exception occurred during compilation. Please check the logs.", "Ok");
                        }
                        Close();
                    }
                }
            }
        }
    }
}
#endif
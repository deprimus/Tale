#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace TaleUtil
{
    public partial class Editor
    {
        public class CompileStoryDialog : EditorWindow
        {
            public string file = null;
            GUIStyle labelStyle = null;
            GUIStyle browseButtonStyle = null;
            GUIStyle compileButtonStyle = null;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = false
                    };
                }

                EditorGUILayout.LabelField(file ?? "<Select story file>", labelStyle);

                GUILayout.Space(5f);

                if (browseButtonStyle == null)
                {
                    browseButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 14,
                        fixedHeight = 23,
                    };
                }

                if (GUILayout.Button("Browse", browseButtonStyle))
                {
                    string path = EditorUtility.OpenFilePanelWithFilters("Select Story File", System.IO.Path.Combine(Application.dataPath, "Scripts"), new string[] { "Tale story files", "md" });
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        file = path;
                    }
                }

                EditorGUILayout.Space(5);

                if (compileButtonStyle == null)
                {
                    compileButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 16,
                        fixedHeight = 30,
                    };
                }

                GUI.enabled = !string.IsNullOrEmpty(file);

                if (GUILayout.Button("Compile Story", compileButtonStyle) || (Event.current.keyCode == KeyCode.Return && GUI.enabled))
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

                GUI.enabled = true;
            }
        }
    }
}
#endif
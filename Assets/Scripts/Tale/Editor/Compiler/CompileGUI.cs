#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleUtil
{
    public partial class Editor
    {
        public class CompileStoryDialog : EditorWindow
        {
            [SerializeField]
            string file = null;

            [SerializeField]
            List<string> scenes = null;

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

                // Step 1: Compile the story script and generate C# scripts for each scene
                // Step 2: Wait for Unity to compile those C# scripts, and then create Unity scenes

                // This is Step 2
                if (scenes != null)
                {
                    // Since Unity compiles stuff in the background, we have to make sure that it's done.
                    // CompilationPipeline events don't work for some reason; the asset is loaded but .GetClass() is always null.
                    // However, upon second execution, it works as expected, so it means that the scripts weren't properly reloaded
                    // the first time by Unity (so we need to wait a bit longer).
                    //
                    // The only thing that works reliably is just busy waiting like this.
                    var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(string.Format("Assets/Scripts/Scenes/{0}.cs", scenes[0]));

                    if (asset != null && asset.GetClass() != null)
                    {
                        Log.Info("Compiler", "Creating scenes");

                        foreach (var s in scenes)
                        {
                            CreateStoryScene(s, string.Format("Assets/Scripts/Scenes/{0}.cs", s));
                        }

                        Log.Info("Compiler", "Story compiled successfully");

                        Close();
                    }
                }

                if (scenes == null)
                {
                    EditorGUILayout.LabelField(file ?? "<Select story file>", labelStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("Waiting for the C# scripts to be compiled by Unity...", labelStyle);
                }

                GUILayout.Space(5f);

                if (scenes == null)
                {
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

                    // Step 1
                    if (GUILayout.Button("Compile Story", compileButtonStyle) || (Event.current.keyCode == KeyCode.Return && GUI.enabled))
                    {
                        try
                        {
                            var result = StoryCompiler.Compile(file, "Assets/Scripts");

                            if (result == null)
                            {
                                EditorUtility.DisplayDialog("Compilation failed", "An error occurred during compilation. Please check the logs.", "Ok");
                            }
                            else
                            {
                                scenes = result.ToList();

                                if (scenes.Count == 0)
                                {
                                    EditorUtility.DisplayDialog("No scenes compiled", "The given script has no scenes. Nothing was compiled", "Ok");
                                    Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Compiler", ex.ToString());
                            EditorUtility.DisplayDialog("Compilation failed", "An exception occurred during compilation. Please check the logs.", "Ok");
                        }
                    }

                    GUI.enabled = true;
                }
            }
        }
    }
}
#endif
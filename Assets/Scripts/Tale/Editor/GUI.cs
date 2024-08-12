#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleUtil
{
    public partial class Editor
    {
        public class RunSetupDialog : EditorWindow
        {
            bool advancedMode = false;
            GUIStyle setupButtonStyle = null;
            GUIStyle customSetupLabelStyle = null;
            GUIStyle customSetupCategoryStyle = null;

            bool setupDialog = true;
            bool setupAudio = true;
            bool setupTransitions = true;
            bool setupCinematic = true;
            bool setupDebug = true;

            bool setupSplashScene = true;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                {
                    advancedMode = EditorGUILayout.ToggleLeft("I know what I'm doing", advancedMode, GUILayout.Width(145));
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (advancedMode)
                {
                    minSize = new Vector2(400f, 365f);

                    GUILayout.Space(10f);

                    if (customSetupLabelStyle == null)
                    {
                        customSetupLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            wordWrap = false,
                            fontSize = 16
                        };
                    }

                    if (customSetupCategoryStyle == null)
                    {
                        customSetupCategoryStyle = new GUIStyle(EditorStyles.boldLabel)
                        {
                            wordWrap = false,
                            fontSize = 14
                        };
                    }

                    EditorGUILayout.LabelField("Custom setup", customSetupLabelStyle);

                    BeginLine();
                    {
                        EditorGUILayout.LabelField("Modules", customSetupCategoryStyle, GUILayout.Width(62));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Options", customSetupCategoryStyle, GUILayout.Width(57));
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupDialog = EditorGUILayout.ToggleLeft("Dialog", setupDialog, GUILayout.Width(55));
                        GUILayout.FlexibleSpace();

                        setupSplashScene = EditorGUILayout.ToggleLeft("Splash scene", setupSplashScene, GUILayout.Width(95));
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupAudio = EditorGUILayout.ToggleLeft("Audio", setupAudio, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupTransitions = EditorGUILayout.ToggleLeft("Transitions", setupTransitions, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupCinematic = EditorGUILayout.ToggleLeft("Cinematic", setupCinematic, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupDebug = EditorGUILayout.ToggleLeft("Debug", setupDebug, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    GUILayout.Space(10f);
                }
                else
                {
                    minSize = new Vector2(400f, 205f);
                }

                maxSize = minSize;

                EditorGUILayout.Space(5f);

                if (setupButtonStyle == null)
                {
                    setupButtonStyle = new GUIStyle(GUI.skin.button) {
                        fontSize = 20,
                        fixedHeight = 40,
                    };
                }

                if (advancedMode && setupSplashScene && (!setupTransitions || !setupAudio))
                {
                    EditorGUILayout.HelpBox("Splash Scene requires the Transitions and Audio modules", MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("Run Setup", setupButtonStyle) || Event.current.keyCode == KeyCode.Return)
                    {
                        if (advancedMode)
                        {
                            SetupCreateMasterObject(setupDialog, setupAudio, setupTransitions, setupCinematic, setupDebug);
                        }
                        else
                        {
                            SetupCreateMasterObject();
                        }

                        if (!advancedMode || setupSplashScene)
                        {
                            SetupCreateSplashScene();
                        }

                        Close();
                    }
                }
            }

            void BeginLine()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
            }

            void EndLine()
            {
                GUILayout.Space(30f);
                EditorGUILayout.EndHorizontal();
            }
        }

        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;
            GUIStyle createButtonStyle = null;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 15f);

                name = EditorGUILayout.TextField("Transition Name:", name);

                EditorGUILayout.Space(8);

                if (createButtonStyle == null)
                {
                    createButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 16,
                        fixedHeight = 30,
                    };
                }

                GUI.enabled = !string.IsNullOrEmpty(name);

                if (GUILayout.Button("Create Transition", createButtonStyle) || (Event.current.keyCode == KeyCode.Return && GUI.enabled))
                {
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(TALE_PREFAB_PATH))
                    {
                        CreateTaleTransition(scope.prefabContentsRoot, name);
                    }

                    Close();
                }

                GUI.enabled = true;
            }
        }

        public class CreateSplashSceneDialog : EditorWindow
        {
            public new string name;
            GUIStyle createButtonStyle = null;
            public Sprite logo;
            public List<AudioClip> soundVariants;

            void OnGUI()
            {
                float logoWidth = position.width - 2 * 50f;
                float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

                EditorGUI.DrawRect(new Rect(0f, 0f, position.width, logoHeight + 5f), Color.black);
                GUI.DrawTexture(new Rect(50f, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

                GUILayout.Space(logoHeight + 10f);

                name = EditorGUILayout.TextField("Splash Scene name:", name);

                EditorGUILayout.Space(20);

                logo = (Sprite) EditorGUILayout.ObjectField("Splash logo:", logo, typeof(Sprite), false);

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Splash Sound Variants", EditorStyles.boldLabel);
                    if (GUILayout.Button("Add Splash Sound", GUILayout.Width(130f)))
                    {
                        if (soundVariants == null)
                        {
                            soundVariants = new List<AudioClip>();
                        }

                        soundVariants.Add(null);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                if (soundVariants != null)
                {
                    for (int i = 0; i < soundVariants.Count;)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            soundVariants[i] = (AudioClip)EditorGUILayout.ObjectField("Sound " + i, soundVariants[i], typeof(AudioClip), false);

                            if (GUILayout.Button("Remove", GUILayout.Width(65f)))
                            {
                                soundVariants.RemoveAt(i);
                                continue;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        ++i;
                    }
                }

                EditorGUI.indentLevel--;

                minSize = new Vector2(minSize.x, 315f + 21 * (soundVariants != null ? soundVariants.Count : 0));

                EditorGUILayout.Space(5);

                if (createButtonStyle == null)
                {
                    createButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontSize = 16,
                        fixedHeight = 30,
                    };
                }

                GUI.enabled = !string.IsNullOrEmpty(name);

                if (GUILayout.Button("Create Splash Scene", createButtonStyle) || (Event.current.keyCode == KeyCode.Return && GUI.enabled))
                {
                    CreateSplashScene(name, logo, soundVariants);
                    Close();
                }

                GUI.enabled = true;
            }
        }
    }
}
#endif
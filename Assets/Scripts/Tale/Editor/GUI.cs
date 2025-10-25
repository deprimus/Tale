#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace TaleUtil
{
    public partial class Editor
    {
#if UNITY_6000_0_OR_NEWER
        const float GUI_EXTRA_WIDTH = 10f;
        const float GUI_EXTRA_HEIGHT = 9f;
#else
        const float GUI_EXTRA_WIDTH = 0f;
        const float GUI_EXTRA_HEIGHT = 0f;
#endif
        const float TALE_LOGO_MAX_WIDTH = 400f;

        public static void DrawTaleLogo(Rect window)
        {
            float logoWidth = Mathf.Min(window.width - 2 * 50f, TALE_LOGO_MAX_WIDTH);
            float logoHeight = logoWidth / 2.31f; // Tale logo aspect ratio: 2.31

            float logoOffset = (window.width - logoWidth) / 2f;

            EditorGUI.DrawRect(new Rect(0f, 0f, window.width, logoHeight + 5f), Color.black);
            GUI.DrawTexture(new Rect(logoOffset, 0f, logoWidth, logoHeight), AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Tale/Logo.png"), ScaleMode.ScaleToFit);

            GUILayout.Space(logoHeight + 15f);
        }

        public class RunSetupDialog : EditorWindow
        {
            bool advancedMode = false;
            GUIStyle setupButtonStyle = null;
            GUIStyle customSetupLabelStyle = null;
            GUIStyle customSetupCategoryStyle = null;

            bool setupDialog = true;
            bool setupAudio = true;
            bool setupTransitions = true;
            bool setupChoice = true;
            bool setupCinematic = true;
            bool setupDebug = true;

            bool setupSplashScene = true;

            bool setupSceneSelector = true;

            void OnGUI()
            {
                DrawTaleLogo(position);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                {
                    advancedMode = EditorGUILayout.ToggleLeft("I know what I'm doing", advancedMode, GUILayout.Width(145));
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (advancedMode)
                {
                    minSize = new Vector2(400f + GUI_EXTRA_WIDTH, 390f + GUI_EXTRA_HEIGHT);

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
                        EditorGUILayout.LabelField("Modules", customSetupCategoryStyle, GUILayout.Width(62 + GUI_EXTRA_WIDTH));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Options", customSetupCategoryStyle, GUILayout.Width(57));
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupDialog = EditorGUILayout.ToggleLeft(new GUIContent("Dialog", tooltip: "Dialog system (required for Tale.Dialog)."), setupDialog, GUILayout.Width(55 + GUI_EXTRA_WIDTH));
                        GUILayout.FlexibleSpace();

                        setupSplashScene = EditorGUILayout.ToggleLeft(new GUIContent("Splash Scene", tooltip: "Create the Tale splash scene, and add it to the build."), setupSplashScene, GUILayout.Width(95 + GUI_EXTRA_WIDTH));
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupAudio = EditorGUILayout.ToggleLeft(new GUIContent("Audio", tooltip: "Audio system (required for Tale.Sound, Tale.Music, and dialog voices)."), setupAudio, GUILayout.Width(80 + GUI_EXTRA_WIDTH));
                        GUILayout.FlexibleSpace();

                        setupSceneSelector = EditorGUILayout.ToggleLeft(new GUIContent("Scene Selector", tooltip: "Create and enable the scene selector (F12 key)."), setupSceneSelector, GUILayout.Width(95 + GUI_EXTRA_WIDTH));
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupTransitions = EditorGUILayout.ToggleLeft(new GUIContent("Transition", tooltip: "Transition system (required for Tale.TransitionIn/Out)."), setupTransitions, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupChoice = EditorGUILayout.ToggleLeft(new GUIContent("Choice", tooltip: "Choice system (required for Tale.Choice)."), setupChoice, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupCinematic = EditorGUILayout.ToggleLeft(new GUIContent("Cinematic", tooltip: "Cinematic system (required for Tale.Cinema.*)."), setupCinematic, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        setupDebug = EditorGUILayout.ToggleLeft(new GUIContent("Debug", tooltip: "Debug system (F3 key)."), setupDebug, GUILayout.Width(80));
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    GUILayout.Space(10f);
                }
                else
                {
                    minSize = new Vector2(400f + GUI_EXTRA_WIDTH, 210f + GUI_EXTRA_HEIGHT);
                }

                maxSize = new Vector2(minSize.x + 0.001f, minSize.y + 0.001f); // Unity ignores maxSize if it's exactly the same as minSize

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
                            SetupCreateMasterObject(setupDialog, setupAudio, setupTransitions, setupChoice, setupCinematic, setupDebug);
                        }
                        else
                        {
                            SetupCreateMasterObject();
                        }

                        if (!advancedMode || setupSplashScene)
                        {
                            SetupCreateSplashScene();
                        }

                        if (!advancedMode || setupSceneSelector)
                        {
                            SetupSceneSelector();
                        }

                        Close();
                    }
                }
            }

            void BeginLine()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f + GUI_EXTRA_WIDTH / 2f);
            }

            void EndLine()
            {
                GUILayout.Space(30f + GUI_EXTRA_WIDTH / 2f);
                EditorGUILayout.EndHorizontal();
            }
        }

        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;
            GUIStyle createButtonStyle = null;

            void OnGUI()
            {
                DrawTaleLogo(position);

                minSize = new Vector2(400f + GUI_EXTRA_WIDTH, 203f + GUI_EXTRA_HEIGHT);
                maxSize = new Vector2(minSize.x + 0.001f, minSize.y + 0.001f); // Unity ignores maxSize if it's exactly the same as minSize

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
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(TALE_MASTER_PREFAB_PATH))
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
                DrawTaleLogo(position);

                minSize = new Vector2(400f + GUI_EXTRA_WIDTH, 321f + GUI_EXTRA_HEIGHT + 21f * (soundVariants != null ? soundVariants.Count : 0f));
                maxSize = new Vector2(minSize.x + 0.001f, minSize.y + 0.001f); // Unity ignores maxSize if it's exactly the same as minSize

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
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        ++i;
                    }
                }

                EditorGUI.indentLevel--;

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
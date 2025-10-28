#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TaleUtil.Scripts.Choice.Default;

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

        static readonly Color TALE_COLOR_RED = new Color(255f / 255f, 20f / 255f, 20f / 255f);
        static readonly Color TALE_COLOR_GREEN = new Color(20f / 255f, 255f / 255f, 20f / 255f);

        class SetupFlag {
            public bool was;
            public bool should;

            static SetupFlag setUp = new SetupFlag() { was = false, should = true };

            public void Set(bool value) {
                was = value;
                should = value;
            }

            public bool HasChanged() {
                return was != should;
            }

            public static implicit operator SetupFlag(bool value) =>
                new SetupFlag() { was = !value, should = value };
        }

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
            enum State {
                SETUP,
                ADVANCED_SETUP,
                MODIFY
            }

            State state = State.SETUP;

            GUIStyle setupButtonStyle = null;
            GUIStyle customSetupLabelStyle = null;
            GUIStyle customSetupCategoryStyle = null;

            GUIStyle greenLabelStyle = null;
            GUIStyle redLabelStyle = null;

            SetupFlag setupDialog = true;
            SetupFlag setupAudio = true;
            SetupFlag setupTransitions = true;
            SetupFlag setupChoice = true;
            SetupFlag setupCinematic = true;
            SetupFlag setupDebug = true;

            SetupFlag setupSplashScene = true;
            SetupFlag setupSceneSelector = true;

            public void Init() {
                if (File.Exists(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB)) {
                    state = State.MODIFY;

                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB);

                    var master = obj.GetComponent<TaleMaster>();

                    setupDialog.Set(master.props.dialogCanvas != null);
                    setupAudio.Set(master.props.audioGroup != null);
                    setupTransitions.Set(master.props.transitions != null && master.props.transitions.Length > 0);
                    setupChoice.Set(master.props.choiceStyles != null && master.props.choiceStyles.Length > 0);
                    setupCinematic.Set(master.props.cinematicCanvas != null);
                    setupDebug.Set(master.props.debugMaster != null);

                    setupSplashScene.Set(File.Exists(GetSplashScenePath("Tale")));
                    setupSceneSelector.Set(File.Exists(TaleUtil.Config.Editor.RESOURCE_SCENE_SELECTOR_ITEM_PREFAB));
                }
            }

            void OnGUI()
            {
                if (Event.current.keyCode == KeyCode.Escape) {
                    Close();
                    return;
                }

                DrawTaleLogo(position);

                InitStyles();

                float headingSize = 0f;

                if (state != State.MODIFY) {
                    headingSize = 20f;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    {
                        state = EditorGUILayout.ToggleLeft("I know what I'm doing", state == State.ADVANCED_SETUP, GUILayout.Width(145 + GUI_EXTRA_WIDTH)) ? State.ADVANCED_SETUP : State.SETUP;
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }

                if (state != State.SETUP)
                {
                    minSize = new Vector2(400f + GUI_EXTRA_WIDTH, 370f + headingSize + GUI_EXTRA_HEIGHT);

                    GUILayout.Space(10f);

                    EditorGUILayout.LabelField(state == State.MODIFY ? "Modify Tale" : "Custom setup", customSetupLabelStyle);

                    BeginLine();
                    {
                        EditorGUILayout.LabelField("Modules", customSetupCategoryStyle, GUILayout.Width(62 + GUI_EXTRA_WIDTH));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Options", customSetupCategoryStyle, GUILayout.Width(57));
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupDialog, "Dialog", "Dialog system (required for Tale.Dialog).", 55);
                        GUILayout.FlexibleSpace();
                        DrawCheckbox(setupSplashScene, "Splash Scene", "Create the Tale splash scene, and add it to the build.", 95);
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupAudio, "Audio", "Audio system (required for Tale.Sound, Tale.Music, and dialog voices).", 80);
                        GUILayout.FlexibleSpace();
                        DrawCheckbox(setupSceneSelector, "Scene Selector", "Create and enable the scene selector (F12 key).", 95);
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupTransitions, "Transition", "Transition system (required for Tale.TransitionIn/Out).", 80);
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupChoice, "Choice", "Choice system (required for Tale.Choice).", 80);
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupCinematic, "Cinematic", "Cinematic system (required for Tale.Cinema.*).", 80);
                        GUILayout.FlexibleSpace();
                    }
                    EndLine();

                    BeginLine();
                    {
                        DrawCheckbox(setupDebug, "Debug", "Debug system (F3 key).", 80);
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

                if (state != State.SETUP && setupSplashScene.should && (!setupTransitions.should || !setupAudio.should))
                {
                    EditorGUILayout.HelpBox("Splash Scene requires the Transition and Audio modules", MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button(state == State.MODIFY ? "Modify" : "Run Setup", setupButtonStyle) || Event.current.keyCode == KeyCode.Return)
                    {
                        SetupCreateMasterObject(setupDialog, setupAudio, setupTransitions, setupChoice, setupCinematic, setupDebug);
                        SetupTaleSplashScene(setupSplashScene);
                        SetupSceneSelector(setupSceneSelector);

                        AssetDatabase.Refresh();

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

            void DrawCheckbox(SetupFlag val, string label, string tooltip, int width) {
                val.should = EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip: tooltip), val.should, GetLabelStyle(val), GUILayout.Width(width + GUI_EXTRA_WIDTH));
            }

            void InitStyles() {
                if (setupButtonStyle == null) {
                    customSetupLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = false,
                        fontSize = 16
                    };

                    customSetupCategoryStyle = new GUIStyle(EditorStyles.boldLabel) {
                        wordWrap = false,
                        fontSize = 14
                    };

                    setupButtonStyle = new GUIStyle(GUI.skin.button) {
                        fontSize = 20,
                        fixedHeight = 40,
                    };

                    greenLabelStyle = new GUIStyle(EditorStyles.label) {
                        normal = {
                            textColor = TALE_COLOR_GREEN
                        },
                        active = {
                            textColor = TALE_COLOR_GREEN
                        },
                        focused = {
                            textColor = TALE_COLOR_GREEN
                        },
                        hover = {
                            textColor = TALE_COLOR_GREEN
                        }
                    };

                    redLabelStyle = new GUIStyle(EditorStyles.label) {
                        normal = {
                            textColor = TALE_COLOR_RED
                        },
                        active = {
                            textColor = TALE_COLOR_RED
                        },
                        focused = {
                            textColor = TALE_COLOR_RED
                        },
                        hover = {
                            textColor = TALE_COLOR_RED
                        }
                    };
                }
            }

            GUIStyle GetLabelStyle(SetupFlag val) {
                if (state != State.MODIFY || val.was == val.should) {
                    return EditorStyles.label;
                }

                if (val.should) {
                    return greenLabelStyle;
                }

                return redLabelStyle;
            }
        }

        public class CreateTransitionDialog : EditorWindow
        {
            public new string name;
            GUIStyle createButtonStyle = null;

            void OnGUI()
            {
                if (Event.current.keyCode == KeyCode.Escape) {
                    Close();
                    return;
                }

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
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(TaleUtil.Config.Editor.RESOURCE_MASTER_PREFAB))
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
                if (Event.current.keyCode == KeyCode.Escape) {
                    Close();
                    return;
                }

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
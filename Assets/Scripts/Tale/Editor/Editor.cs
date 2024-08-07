#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TaleUtil
{
    public partial class Editor
    {
        [MenuItem("Tale/Setup/Run Full Setup", priority = 1)]
        static void FullSetup()
        {
            SetupCreateMasterObject();
            SetupCreateSplashScene();
        }

        [MenuItem("Tale/Setup/Manual Setup", priority = 12)]
        static void ManualSetupDummy() { }

        [MenuItem("Tale/Setup/Manual Setup", true, priority = 12)]
        static bool ManualSetupDummyValidate() => false;

        [MenuItem("Tale/Setup/1. Install Dependencies", priority = 13)]
        static void SetupInstallDependencies()
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        }

        [MenuItem("Tale/Setup/2. Create Master Object", priority = 14)]
        static void SetupCreateMasterObject()
        {
            if (File.Exists(TALE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Tale Master already created", "Tale Master prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TALE_PREFAB_PATH, "Ok");
                return;
            }

            Scene s = EditorSceneManager.GetActiveScene();

            GameObject master = new GameObject("Tale Master", typeof(TaleMaster));

            SetupDialog(master);
            SetupAudio(master);
            SetupAdvance(master);
            SetupTransitions(master);
            SetupCinematic(master);
            SetupDebug(master);

            CreateTag("TaleMaster");
            master.tag = "TaleMaster";

            Undo.RegisterCreatedObjectUndo(master, "Create " + master.name);
            Selection.activeGameObject = master;

            CreateTaleMasterPrefab(master);

            if (s.path != null && s.path.Length > 0)
            {
                EditorSceneManager.SaveScene(s, s.path);
            }
        }

        [MenuItem("Tale/Setup/3. Create Splash Scene", priority = 15)]
        static void SetupCreateSplashScene()
        {
            SetupTaleSplashScene();
        }

        [MenuItem("Tale/Create/Transition", priority = 1)]
        static void CreateTransition()
        {
            if (!TaleWasSetUp())
            {
                EditorUtility.DisplayDialog("Tale not set up", "Please set up Tale before creating transitions:\n\nTale -> Setup -> Run Full Setup", "Ok");
                return;
            }

            if (FindTaleMaster() == null)
            {
                InstantiateTaleMasterPrefab();
            }

            CreateTransitionDialog dialog = EditorWindow.GetWindow<CreateTransitionDialog>();
            dialog.titleContent = new GUIContent("Tale - Create Transition");
            dialog.minSize = new Vector2(400f, 210f);
            dialog.maxSize = dialog.minSize;
            dialog.ShowPopup();
        }

        [MenuItem("Tale/Create/Splash Scene", priority = 2)]
        static void CreateSplashScene()
        {
            CreateSplashSceneDialog dialog = EditorWindow.GetWindow<CreateSplashSceneDialog>();
            dialog.titleContent = new GUIContent("Tale - Create Splash Scene");
            dialog.minSize = new Vector2(400f, 350f);
            dialog.maxSize = new Vector2(400f, 16384f);
            dialog.ShowPopup();
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TaleUtil
{
    public partial class Editor
    {
        [MenuItem("Tale/Setup/Install Dependencies", priority = 1)]
        static void SetupInstallDependencies()
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        }

        [MenuItem("Tale/Setup/Run Full Setup", priority = 2)]
        static void FullSetup()
        {
            SetupCreateMasterObject();
            SetupCreateSplashScene();
        }

        [MenuItem("Tale/Setup/1. Create Master Object", priority = 13)]
        static void SetupCreateMasterObject()
        {
            Scene s = EditorSceneManager.GetActiveScene();

            GameObject master = new GameObject("Tale Master", typeof(TaleMaster));

            SetupDialog(master);
            SetupAudio(master);
            SetupTransitions(master);
            SetupCinematic(master);
            SetupDebug(master);

            CreateTag("TaleMaster");
            master.tag = "TaleMaster";

            Undo.RegisterCreatedObjectUndo(master, "Create " + master.name);
            Selection.activeGameObject = master;

            if (s.path != null && s.path.Length > 0)
            {
                EditorSceneManager.SaveScene(s, s.path);
            }
        }

        [MenuItem("Tale/Setup/2. Create Splash Scene", priority = 14)]
        static void SetupCreateSplashScene()
        {
            SetupTaleSplashScene();
        }

        [MenuItem("Tale/Create/Transition", priority = 1)]
        static void CreateTransition()
        {
            if (FindTaleMaster() == null)
            {
                EditorUtility.DisplayDialog("Tale Master not found in this scene", "Please set up Tale before creating transitions.", "Ok");
            }
            else
            {
                CreateTransitionDialog dialog = EditorWindow.GetWindow<CreateTransitionDialog>();
                dialog.titleContent = new GUIContent("Tale - Create Transition");
                dialog.minSize = new Vector2(400f, 210f);
                dialog.maxSize = dialog.minSize;
                dialog.ShowPopup();
            }
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
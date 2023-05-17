#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TaleUtil
{
    public partial class Editor
    {
        [MenuItem("Tale/Setup/Run Full Setup", priority = 1)]
        static void FullSetup()
        {
            InstallDependencies();
            CreateMasterObject();
        }

        [MenuItem("Tale/Setup/1. Install Dependencies", priority = 12)]
        static void InstallDependencies()
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        }

        [MenuItem("Tale/Setup/2. Create Master Object", priority = 13)]
        static void CreateMasterObject()
        {
            GameObject master = new GameObject("Tale Master", typeof(TaleMaster));

            SetupDialog(master);
            SetupAudio(master);
            SetupTransitions(master);
            SetupCinematic(master);

            CreateTag("TaleMaster");
            master.tag = "TaleMaster";

            Undo.RegisterCreatedObjectUndo(master, "Create " + master.name);
            Selection.activeGameObject = master;
        }

        [MenuItem("Tale/Setup/3. Create Splash Scene", priority = 14)]
        static void CreateSplashScene()
        {

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
                dialog.titleContent = new GUIContent("Create Transition");
                dialog.ShowPopup();
            }
        }
    }
}
#endif
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using System.Collections;

namespace TaleUtil
{
    public partial class Editor
    {
        [MenuItem("Tale/Run Setup", priority = 1)]
        static void RunSetup()
        {
            if (File.Exists(TALE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("Tale Master already created", "Tale Master prefab already exists.\n\nIf you want to regenerate it, delete the prefab at:\n\n" + TALE_PREFAB_PATH, "Ok");
                return;
            }

            RunSetupDialog dialog = EditorWindow.GetWindow<RunSetupDialog>();
            dialog.titleContent = new GUIContent("Tale - Setup");
            dialog.ShowPopup();
        }

        [MenuItem("Tale/Compile Story", priority = 10)]
        static void CompileStory()
        {
            CompileStoryDialog dialog = EditorWindow.GetWindow<CompileStoryDialog>();
            dialog.titleContent = new GUIContent("Tale - Compile Story");
            dialog.minSize = new Vector2(400f, 225f);
            dialog.maxSize = dialog.minSize;
            dialog.ShowPopup();
        }

        [MenuItem("Tale/Create/Transition", priority = 20)]
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
            dialog.minSize = new Vector2(400f, 203f);
            dialog.maxSize = dialog.minSize;
            dialog.ShowPopup();
        }

        [MenuItem("Tale/Create/Splash Scene", priority = 21)]
        static void CreateSplashScene()
        {
            CreateSplashSceneDialog dialog = EditorWindow.GetWindow<CreateSplashSceneDialog>();
            dialog.titleContent = new GUIContent("Tale - Create Splash Scene");
            dialog.minSize = new Vector2(400f, 315f);
            dialog.maxSize = dialog.minSize;            // Force the window to have that initial size
            dialog.maxSize = new Vector2(400f, 16384f); // But let the user expand it afterwards
            dialog.ShowPopup();
        }

        [MenuItem("Tale/Finalize/Scene Selector/Create Scene Selector", priority = 30)]
        static void CreateSceneSelector()
        {
            
        }

        [MenuItem("Tale/Finalize/Scene Selector/Auto-Generate Scene Thumbnails", priority = 31)]
        static void AutoGenerateSceneThumbnails()
        {
            CaptureSceneThumbnails();
        }

        [MenuItem("Tale/Finalize/Scene Selector/Generate Current Scene Thumbnail _F11", priority = 32)]
        static void GenerateSceneThumbnail()
        {
            SceneThumbnailGenerator.CaptureThumbnail();
        }

        [MenuItem("Tale/Debug/Create Full Master Object", priority = 50)]
        static void SetupCreateMasterObjectMenu()
        {
            SetupCreateMasterObject();
        }

        [MenuItem("Tale/Debug/Create Tale Splash Scene", priority = 51)]
        static void SetupCreateSplashScene()
        {
            SetupTaleSplashScene();
        }
    }
}
#endif
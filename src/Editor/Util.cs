#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Animations;

namespace TaleUtil
{
    public partial class Editor
    {
        static GameObject FindTaleMaster()
        {
            if (!TagExists("TaleMaster"))
            {
                return null;
            }

            return GameObject.FindGameObjectWithTag("TaleMaster");
        }

        static bool TagExists(string name)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(name);
        }

        static void CreateTag(string name)
        {
            if (!TagExists(name))
            {
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                int index = tagsProp.arraySize;
                tagsProp.InsertArrayElementAtIndex(index);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(index);
                newTag.stringValue = name;

                tagManager.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static GameObject CreateAudioSource(string name)
        {
            GameObject obj = new GameObject(name);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.priority = 0;
            src.dopplerLevel = 0;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.maxDistance = 10100;
            src.minDistance = 10000;

            return obj;
        }

        static GameObject CreateDarkness(string name)
        {
            GameObject darkness = new GameObject(name);
            Image image = darkness.AddComponent<Image>();
            image.color = Color.black;

            StretchTransform(darkness.GetComponent<RectTransform>());

            return darkness;
        }

        static void CreateTaleTransition(GameObject master, string name)
        {
            TaleMaster tale = master.GetComponent<TaleMaster>();

            // Fade
            GameObject canvas = CreateCanvas(string.Format("Transition {0} Canvas", name), Config.TRANSITION_SORT_ORDER);
            GameObjectUtility.SetParentAndAlign(canvas, master);

            Animator anim = AddAnimator(canvas);

            canvas.SetActive(false);

            GameObject darkness = CreateDarkness("Darkness");
            darkness.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            GameObjectUtility.SetParentAndAlign(darkness, canvas);

            CreateCompleteTriangleAnimator(anim, string.Format("Transition{0}", name),
                string.Format(Config.TRANSITION_ANIMATOR_STATE_FORMAT, "In"),
                string.Format(Config.TRANSITION_ANIMATOR_STATE_FORMAT, "Out"),
                string.Format(Config.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "In"),
                string.Format(Config.TRANSITION_ANIMATOR_TRIGGER_FORMAT, "Out"),
                Config.TRANSITION_ANIMATOR_TRIGGER_NEUTRAL,
                "Darkness", typeof(Image), "m_Color.a",
                AnimationCurve.Linear(0f, 1f, 1f, 0f),
                AnimationCurve.Linear(0f, 0f, 1f, 1f));

            if (tale.transitions == null)
            {
                tale.transitions = new Props.Transition[1];
            }
            else
            {
                Props.Transition[] transitions = new Props.Transition[tale.transitions.Length + 1];
                System.Array.Copy(tale.transitions, transitions, tale.transitions.Length);

                tale.transitions = transitions;
            }

            int index = tale.transitions.Length - 1;

            tale.transitions[index] = new Props.Transition();
            tale.transitions[index].name = name;
            tale.transitions[index].data = new Props.TransitionData(canvas, canvas.GetComponent<Animator>());
        }

        // Generates the states, triggers and clips for an Animator which are required by Tale
        static void CreateCompleteTriangleAnimator(Animator anim, string controllerName, string stateIn, string stateOut, string triggerIn, string triggerOut, string triggerNeutral, string animatedPath, System.Type animatedType, string animatedProperty, AnimationCurve curveIn, AnimationCurve curveOut)
        {
            string root = "Assets/Animations/Tale";
            Directory.CreateDirectory(root);

            AnimatorController ctrl = new AnimatorController();
            ctrl.name = controllerName;

            ctrl.AddLayer("Base Layer");

            ctrl.AddParameter(triggerIn, AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter(triggerOut, AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter(triggerNeutral, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine states = ctrl.layers[0].stateMachine;

            AnimatorState idle = states.AddStateNoWriteDefaults("Idle");
            states.defaultState = idle;

            AnimatorState dialogIn = states.AddStateNoWriteDefaults(stateIn);

            AnimationClip clip = new AnimationClip();
            clip.name = controllerName + "In";
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveIn);
            AssetDatabase.CreateAsset(clip, Path.Enroot(root, clip.name + ".anim"));

            dialogIn.motion = clip;

            AnimatorState dialogOut = states.AddStateNoWriteDefaults(stateOut);

            clip = new AnimationClip();
            clip.name = controllerName + "Out";
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveOut);
            AssetDatabase.CreateAsset(clip, Path.Enroot(root, clip.name + ".anim"));

            dialogOut.motion = clip;

            CreateAnimatorTransitions(idle, dialogIn, dialogOut, triggerIn, triggerOut, triggerNeutral);

            AssetDatabase.CreateAsset(ctrl, Path.Enroot(root, controllerName + ".controller"));
            anim.runtimeAnimatorController = ctrl;
        }

        static void CreateCompleteLoopingAnimator(Animator anim, string controllerName, string state, string animatedPath, System.Type animatedType, string animatedProperty, AnimationCurve curve)
        {
            string root = "Assets/Animations/Tale";
            Directory.CreateDirectory(root);

            AnimatorController ctrl = new AnimatorController();
            ctrl.name = controllerName;

            ctrl.AddLayer("Base Layer");

            AnimatorStateMachine states = ctrl.layers[0].stateMachine;

            AnimatorState idle = states.AddStateNoWriteDefaults(state);
            states.defaultState = idle;

            AnimationClip clip = new AnimationClip();
            clip.name = state;
            clip.SetLoop(true);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curve);
            AssetDatabase.CreateAsset(clip, Path.Enroot(root, state + ".anim"));

            idle.motion = clip;

            AssetDatabase.CreateAsset(ctrl, Path.Enroot(root, controllerName + ".controller"));
            anim.runtimeAnimatorController = ctrl;
        }

        static void StretchTransform(RectTransform tform)
        {
            tform.anchorMin = new Vector2(0f, 0f);
            tform.anchorMax = new Vector2(1f, 1f);
            tform.sizeDelta = new Vector2(0f, 0f);
        }

        static Animator AddAnimator(GameObject obj)
        {
            Animator anim = obj.AddComponent<Animator>();
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;

            return anim;
        }

        static void CreateAnimatorTransitions(AnimatorState idle, AnimatorState stateIn, AnimatorState stateOut, string triggerIn, string triggerOut, string triggerNeutral)
        {
            AnimatorStateTransition transition = idle.AddTransition(stateIn);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerIn);

            transition = idle.AddTransition(stateOut);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerOut);

            transition = stateIn.AddTransition(idle);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerNeutral);

            transition = stateOut.AddTransition(idle);
            transition.hasExitTime = false;
            transition.duration = 0f;
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerNeutral);
        }

        static GameObject CreateCanvas(string name, int sortOrder)
        {
            if (!Object.FindObjectOfType<EventSystem>())
            {
                GameObject system = new GameObject("EventSystem", typeof(EventSystem));
                system.AddComponent<StandaloneInputModule>();
            }

            GameObject obj = new GameObject(name);

            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Config.REFERENCE_WIDTH, Config.REFERENCE_HEIGHT);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100;

            return obj;
        }
    }
}
#endif
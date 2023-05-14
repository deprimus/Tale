#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Animations;

namespace TaleUtil
{
    public partial class Editor
    {
        static GameObject CreateTransition(string name)
        {
            GameObject canvas = CreateCanvas(name, Config.TRANSITION_SORT_ORDER);
            AddAnimator(canvas);

            return canvas;
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
            clip.name = stateIn;
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveIn);
            AssetDatabase.CreateAsset(clip, Path.Enroot(root, stateIn + ".anim"));

            dialogIn.motion = clip;

            AnimatorState dialogOut = states.AddStateNoWriteDefaults(stateOut);

            clip = new AnimationClip();
            clip.name = stateOut;
            clip.SetLoop(false);
            clip.SetCurve(animatedPath, animatedType, animatedProperty, curveOut);
            AssetDatabase.CreateAsset(clip, Path.Enroot(root, stateOut + ".anim"));

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
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerIn);

            transition = idle.AddTransition(stateOut);
            transition.hasExitTime = false;
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
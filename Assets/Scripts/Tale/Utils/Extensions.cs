using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace TaleUtil
{
    public static class Extensions
    {
        public static void Toggle(this ref bool value)
        {
            value = !value;
        }

        public static void Set<T>(this List<T> list, int index, T value)
        {
            if(index < 0 || index > list.Count)
                throw new System.IndexOutOfRangeException("The index must be 0 <= index <= list size.");
            else if(index == list.Count)
                list.Add(value);
            else list[index] = value;
        }
        
        // These extension methods rely on 'triangle' animators.
        // Idle --trigger1--> state1 --trigger2-->Idle
        // Idle --trigger3--> state2 --trigger2-->Idle
        // etc
        public static bool StateFinished(this Animator animator, string state)
        {
            return animator.GetCurrentAnimatorStateInfo(0).StateFinished(state);
        }
        public static bool StateFinished(this AnimatorStateInfo info, string state)
        {
            return (info.IsName(state) && info.normalizedTime >= 1f);
        }

        public static Vector2 Div(this Vector2 left, Vector2 right)
        {
            return new Vector2(right.x == 0 ? left.x : left.x / right.x, right.y == 0 ? left.y : left.y / right.y);
        }

        public static Vector3 Div2D(this Vector3 left, Vector3 right)
        {
            return new Vector3(right.x == 0 ? left.x : left.x / right.x, right.y == 0 ? left.y : left.y / right.y, left.z);
        }

#if UNITY_EDITOR
        public static AnimatorState AddStateNoWriteDefaults(this AnimatorStateMachine machine, string name)
        {
            AnimatorState state = machine.AddState(name);
            state.writeDefaultValues = false;

            return state;
        }

        public static void SetLoop(this AnimationClip clip, bool loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }
#endif

        public static bool HasState(this Animator animator, string state)
        {
#if UNITY_EDITOR
            AnimatorController ctrl = animator.runtimeAnimatorController as AnimatorController;

            if (!ctrl)
            {
                return false;
            }

            int hash = Animator.StringToHash(state);

            foreach (AnimatorControllerLayer layer in ctrl.layers)
            {
                if (layer.stateMachine == null)
                {
                    return false;
                }

                foreach (ChildAnimatorState st in layer.stateMachine.states)
                {
                    if (st.state.nameHash == hash)
                    {
                        return true;
                    }
                }
            }

            return false;
#else
            return true;
#endif
        }

        public static bool HasTrigger(this Animator animator, string state)
        {
#if UNITY_EDITOR
            AnimatorController ctrl = animator.runtimeAnimatorController as AnimatorController;

            if (!ctrl)
            {
                return false;
            }

            foreach (AnimatorControllerParameter param in ctrl.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger && param.name == state)
                {
                    return true;
                }
            }

            return false;
#else
            return true;
#endif
        }

        // If a state is absent, this will log a warning with a given category and format
        public static bool HasStates(this Animator animator, string logCategory, string warnFormat, params string[] states)
        {
            bool ok = true;

            foreach (string state in states)
            {
                if (!animator.HasState(state))
                {
                    Log.Warning(logCategory, string.Format(warnFormat, state));
                    ok = false;
                    // Don't return right away; give the user all of the warnings,
                    // so he can fix them in one go
                }
            }

            return ok;
        }

        // If a trigger is absent, this will log a warning with a given category and format
        public static bool HasTriggers(this Animator animator, string logCategory, string warnFormat, params string[] triggers)
        {
            bool ok = true;

            foreach (string trigger in triggers)
            {
                if (!animator.HasTrigger(trigger))
                {
                    Log.Warning(logCategory, string.Format(warnFormat, trigger));
                    ok = false;
                    // Don't return right away; give the user all of the warnings,
                    // so he can fix them in one go
                }
            }

            return ok;
        }
    }
}

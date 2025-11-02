using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace TaleUtil
{
    public class Input
    {
        TaleMaster master;

        public Input(TaleMaster master) {
            this.master = master;
        }

        public bool Advance()
        {
            var config = master.Config;

            if (GetMouseButtonUp(0) || GetKey(config.Dialog.KEY_SKIP))
                return true;

            for (int i = 0; i < config.Dialog.KEY_NEXT.Length; ++i)
                if (GetKeyDown(config.Dialog.KEY_NEXT[i]))
                    return true;

            return false;
        }

        public static bool GetMouseButton(int button) {
#if ENABLE_INPUT_SYSTEM
            return GetMouseButtonControl(button).isPressed;
#else
            return UnityEngine.Input.GetMouseButton(button);
#endif
        }

        public static bool GetMouseButtonDown(int button) {
#if ENABLE_INPUT_SYSTEM
            return GetMouseButtonControl(button).wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(button);
#endif
        }

        public static bool GetMouseButtonUp(int button) {
#if ENABLE_INPUT_SYSTEM
            return GetMouseButtonControl(button).wasReleasedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonUp(button);
#endif
        }

        public static bool GetKey(KeyCode key) {
#if ENABLE_INPUT_SYSTEM
            return GetKeyControl(key).isPressed;
#else
            return UnityEngine.Input.GetKey(key);
#endif
        }

        public static bool GetKeyDown(KeyCode key) {
#if ENABLE_INPUT_SYSTEM
            return GetKeyControl(key).wasPressedThisFrame;
#else
            return UnityEngine.Input.GetKeyDown(key);
#endif
        }

        public static bool GetKeyUp(KeyCode key) {
#if ENABLE_INPUT_SYSTEM
            return GetKeyControl(key).wasReleasedThisFrame;
#else
            return UnityEngine.Input.GetKeyUp(key);
#endif
        }

        public static bool AnyModPressed() {
            return GetKey(KeyCode.LeftShift)    ||
                   GetKey(KeyCode.RightShift)   ||
                   GetKey(KeyCode.LeftControl)  ||
                   GetKey(KeyCode.RightControl) ||
                   GetKey(KeyCode.LeftAlt)      ||
                   GetKey(KeyCode.RightAlt)     ||
                   GetKey(KeyCode.LeftWindows)  ||
                   GetKey(KeyCode.RightWindows);
        }

#if ENABLE_INPUT_SYSTEM
        static ButtonControl GetMouseButtonControl(int button) {
            var mouse = Mouse.current;

            switch (button) {
                case 0: {
                    return mouse.leftButton;
                }
                case 1: {
                    return mouse.rightButton;
                }
                case 2: {
                    return mouse.middleButton;
                }
                default: {
                    TaleUtil.Log.Error("INPUT", string.Format("Unknown mouse button '{0}'; expected '0' (left), '1' (right), or '2' (middle)", button));
                    return null;
                }
            }
        }

        static KeyControl GetKeyControl(KeyCode key) {
            return Keyboard.current[KeyCodeToKey(key)];
        }

        // I do not like this, but there is no other efficient way to convert KeyCode to Key.
        // Since KeyCode is part of UnityEngine and doesn't depend on the input system (new/legacy),
        // it's what the Tale Config works with, and therefore it's what TaleUtil.Input requires.
        //
        // TaleUtil.Input (KeyCode) |--[Old Input System]--> UnityEngine.Input (KeyCode)
        //                          |--[New Input System]--> KeyCodeToKey() -> UnityEngine.InputSystem (Key)
        static Key KeyCodeToKey(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;

                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;

                case KeyCode.F1: return Key.F1;
                case KeyCode.F2: return Key.F2;
                case KeyCode.F3: return Key.F3;
                case KeyCode.F4: return Key.F4;
                case KeyCode.F5: return Key.F5;
                case KeyCode.F6: return Key.F6;
                case KeyCode.F7: return Key.F7;
                case KeyCode.F8: return Key.F8;
                case KeyCode.F9: return Key.F9;
                case KeyCode.F10: return Key.F10;
                case KeyCode.F11: return Key.F11;
                case KeyCode.F12: return Key.F12;

                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;

                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.LeftCommand:
                case KeyCode.LeftWindows: return Key.LeftMeta;
                case KeyCode.RightCommand:
                case KeyCode.RightWindows: return Key.RightMeta;

                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Delete: return Key.Delete;
                case KeyCode.Insert: return Key.Insert;
                case KeyCode.Home: return Key.Home;
                case KeyCode.End: return Key.End;
                case KeyCode.PageUp: return Key.PageUp;
                case KeyCode.PageDown: return Key.PageDown;
                case KeyCode.CapsLock: return Key.CapsLock;
                case KeyCode.ScrollLock: return Key.ScrollLock;
                case KeyCode.Pause: return Key.Pause;
                case KeyCode.Numlock: return Key.NumLock;
                case KeyCode.Print: return Key.PrintScreen;

                case KeyCode.Keypad0: return Key.Numpad0;
                case KeyCode.Keypad1: return Key.Numpad1;
                case KeyCode.Keypad2: return Key.Numpad2;
                case KeyCode.Keypad3: return Key.Numpad3;
                case KeyCode.Keypad4: return Key.Numpad4;
                case KeyCode.Keypad5: return Key.Numpad5;
                case KeyCode.Keypad6: return Key.Numpad6;
                case KeyCode.Keypad7: return Key.Numpad7;
                case KeyCode.Keypad8: return Key.Numpad8;
                case KeyCode.Keypad9: return Key.Numpad9;
                case KeyCode.KeypadDivide: return Key.NumpadDivide;
                case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
                case KeyCode.KeypadEquals: return Key.NumpadEquals;

                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Equals: return Key.Equals;
                case KeyCode.LeftBracket: return Key.LeftBracket;
                case KeyCode.RightBracket: return Key.RightBracket;
                case KeyCode.Semicolon: return Key.Semicolon;
                case KeyCode.Quote: return Key.Quote;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Backslash: return Key.Backslash;
                case KeyCode.BackQuote: return Key.Backquote;

                default:
                    TaleUtil.Log.Error("INPUT", string.Format("No mapping found from UnityEngine.KeyCode.{0} to UnityEngine.InputSystem.Key; please report this to the devs, and use UnityEngine.InputSystem directly as a temporary solution", keyCode.ToString()));
                    return Key.None;
            }
        }
#endif
    }
}
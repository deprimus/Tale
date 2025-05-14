using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Hooks
    {
        // Called when the dialog state changes
        public static Delegates.CallbackDelegate<DialogAction> OnDialogUpdate;
        // Called when the dialog auto mode is toggled
        public static Delegates.CallbackDelegate<bool> OnDialogAutoModeToggle;

        // Called when triggers are updated. Won't be called if there are no triggers set
        public static Delegates.CallbackDelegate<HashSet<string>> OnTriggerUpdate;

        public static void Init()
        {
            OnDialogUpdate = null;
            OnDialogAutoModeToggle = null;
            OnTriggerUpdate = null;
        }
    }
}
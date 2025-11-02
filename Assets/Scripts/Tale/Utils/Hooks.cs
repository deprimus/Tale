using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public class Hooks
    {
        // Called when the dialog state changes
        public Delegates.CallbackDelegate<DialogAction> OnDialogUpdate;
        // Called when the dialog auto mode is toggled
        public Delegates.CallbackDelegate<bool> OnDialogAutoModeToggle;

        // Called when triggers are updated. Won't be called if there are no triggers set
        public Delegates.CallbackDelegate<HashSet<string>> OnTriggerUpdate;

        public Hooks()
        {
            OnDialogUpdate = null;
            OnDialogAutoModeToggle = null;
            OnTriggerUpdate = null;
        }
    }
}
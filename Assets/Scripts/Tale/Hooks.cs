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
    }
}
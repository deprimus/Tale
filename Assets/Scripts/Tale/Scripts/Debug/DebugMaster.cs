using UnityEngine;

// Before the event system
[DefaultExecutionOrder(-10000)]
public class DebugMaster : MonoBehaviour
{
    public GameObject debugInfo;

    void Update()
    {
        if (Tale.config.DEBUG_INFO_ENABLE && GetKeyDownNoMod(Tale.config.DEBUG_INFO_KEY))
        {
            debugInfo.SetActive(!debugInfo.activeSelf);
        }
    }

    public void ShowDebugInfo()
    {
        debugInfo.SetActive(true);
    }

    static bool GetKeyDownNoMod(KeyCode key)
    {
        return Input.GetKeyDown(key)
            && !Input.GetKeyDown(KeyCode.LeftShift)
            && !Input.GetKeyDown(KeyCode.RightShift)
            && !Input.GetKeyDown(KeyCode.LeftControl)
            && !Input.GetKeyDown(KeyCode.RightControl)
            && !Input.GetKeyDown(KeyCode.LeftAlt)
            && !Input.GetKeyDown(KeyCode.RightAlt)
            && !Input.GetKeyDown(KeyCode.LeftWindows)
            && !Input.GetKeyDown(KeyCode.RightWindows)
            && !Input.GetKeyDown(KeyCode.LeftCommand)
            && !Input.GetKeyDown(KeyCode.RightCommand);
    }
}
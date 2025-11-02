using UnityEngine;

// Before the event system
[DefaultExecutionOrder(-10000)]
public class DebugMaster : MonoBehaviour
{
    public GameObject debugInfo;

    void Update()
    {
        var config = Tale.Master.Config;

        if (config.Core.DEBUG_INFO_ENABLE && GetKeyDownNoMod(config.Core.DEBUG_INFO_KEY))
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
        return TaleUtil.Input.GetKeyDown(key) && !TaleUtil.Input.AnyModPressed();
    }
}
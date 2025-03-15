using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Before Tale actions
[DefaultExecutionOrder(-9999)]
public class DebugInfo : MonoBehaviour
{
    public TextMeshProUGUI fps;
    public TextMeshProUGUI sceneInfo;
    public TextMeshProUGUI actionInfo;
    public TextMeshProUGUI actionCountInfo;

    float freq = 0.5f;
    float clock = 0f;
    int frame = 0;

    void Update()
    {
        UpdateFPS();
        UpdateScene();
        UpdateAction();
        UpdateActionCount();
    }

    void UpdateFPS()
    {
        clock += Time.deltaTime;
        frame++;

        if (clock >= freq)
        {
            fps.text = string.Format("{0} fps", Mathf.RoundToInt(frame / clock));
            frame = 0;
            clock = 0f;
        }
    }

    void UpdateScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        sceneInfo.text = string.Format("{0} ({1})", scene.name, scene.buildIndex);
    }

    void UpdateAction()
    {
        TaleUtil.Action act = TaleUtil.Queue.FetchIfAny();

        if (act != null)
        {
            actionInfo.text = act.ToString();
        }
        else
        {
            actionInfo.text = "";
        }
    }

    void UpdateActionCount()
    {
        actionCountInfo.text = TaleUtil.Queue.GetTotalActionCount().ToString();
    }
}
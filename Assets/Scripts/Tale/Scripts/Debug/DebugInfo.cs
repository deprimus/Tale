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
    public TextMeshProUGUI queueInfo;
    public TextMeshProUGUI parallelInfo;

    float freq = 0.5f;
    float clock = 0f;
    int frame = 0;

    void Update()
    {
        UpdateFPS();
        UpdateScene();
        UpdateAction();
        UpdateActionCount();
        UpdateActionList(queueInfo, Tale.Master.Queue);
        UpdateActionList(parallelInfo, Tale.Master.Parallel);
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
        TaleUtil.Action act = Tale.Master.Queue.FetchIfAny();

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
        actionCountInfo.text = Tale.Master.GetTotalActionCount().ToString();
    }

    void UpdateActionList(TextMeshProUGUI text, System.Collections.Generic.IEnumerable<TaleUtil.Action> list) {
        var sb = new System.Text.StringBuilder();

        foreach (var action in list) {
            DisplayAction(sb, action, 0);
        }

        text.text = sb.ToString();
    }

    void DisplayAction(System.Text.StringBuilder sb, TaleUtil.Action action, uint level) {
        for (int i = 0; i < level; ++i) {
            sb.Append("|   ");
        }

        var color = action.IsRunning ? Tale.Master.Config.Debug.INFO_TEXT_COLOR_SECONDARY : Tale.Master.Config.Debug.INFO_TEXT_COLOR_PRIMARY;

        sb.AppendFormat("<color=#{0}>{1}</color>\n", ColorUtility.ToHtmlStringRGBA(color), action.ToString());

        foreach (var subaction in action.GetSubactions()) {
            DisplayAction(sb, subaction, level + 1);
        }
    }
}
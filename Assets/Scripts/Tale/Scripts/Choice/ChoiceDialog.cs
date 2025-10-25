using System.Threading.Tasks;
using UnityEngine;
using DialogChoiceItem = System.ValueTuple<string, TaleUtil.Delegates.ShallowDelegate>;

public static partial class Tale {
    public static partial class Choice
    {
        public static TaleUtil.Action Dialog(string title, params DialogChoiceItem[] choices) =>
            TaleUtil.Queue.Enqueue(new TaleUtil.ChoiceAction("dialog", title, choices));
    }
}

public class ChoiceDialog : TaleUtil.Scripts.ChoiceMaster
{
    public override void Construct(object args, object choices, TaleUtil.Delegates.ShallowDelegate onEnd)
    {
        var entries = (DialogChoiceItem[])choices;

        TaleUtil.SoftAssert.Condition(entries != null, "Invalid choice item type passed to ChoiceDialog");

        foreach (var entry in entries)
        {
            var label = entry.Item1;
            var callback = entry.Item2;
        }

        Debug.LogWarning("Default dialog style choice picker isn't fully implemented yet; calling first choice callback");
        var cbk = entries[0].Item2;

        if (cbk != null)
        {
            cbk();
        }

        onEnd();
    }
}

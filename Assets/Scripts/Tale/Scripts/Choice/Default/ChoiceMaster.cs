using TaleUtil.Scripts.Choice.Default;
using UnityEngine;
using TMPro;

public static partial class Tale {
    public static partial class Choice {
        public static TaleUtil.Action Default(Args title, params ChoiceItem[] choices) =>
            Style("default", title, choices);
    }
}

namespace TaleUtil.Scripts.Choice.Default {
    // Since Unity doesn't support C# 10+, we have to use records.
    // When C# 10 is available, replace these with "global using"
    public record Args(string title) {
        public static implicit operator Args(string title) =>
            new Args(title);
    }
    public record ChoiceItem(string label, Delegates.ShallowDelegate callback) {
        public static implicit operator ChoiceItem((string, Delegates.ShallowDelegate) args) =>
            new ChoiceItem(args.Item1, args.Item2);
    }

    public class ChoiceMaster : ChoiceMaster<Args, ChoiceItem>
    {
        [SerializeField]
        internal TextMeshProUGUI title;
        [SerializeField]
        internal ChoiceObj[] choiceObjs;

        public override void Present(Args args, ChoiceItem[] choices, Delegates.ShallowDelegate onEnd)
        {
            if (choices == null || choices.Length == 0) {
                Log.Warning("No choices passed to default style choice picker");
                onEnd();
                return;
            }

            if (choices.Length > choiceObjs.Length) {
                Log.Error("CHOICE", string.Format("Default style choice picker supports a maximum of {0} choices, received {1} choices; please create your own choice style, or modify the default one", choiceObjs.Length, choices.Length));
                onEnd();
                return;
            }

            for (int i = 0; i < choiceObjs.Length; i++) {
                var obj = choiceObjs[i];

                if (i < choices.Length) {
                    obj.gameObject.SetActive(true);

                    var callback = choices[i].callback;

                    obj.Present(choices[i].label, () => {
                        if (callback != null) {
                            callback();
                        }

                        onEnd();
                    });
                } else {
                    obj.gameObject.SetActive(false);
                }
            }

            title.text = args.title;
            title.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 93f + choiceObjs[choices.Length - 1].GetComponent<RectTransform>().anchoredPosition.y);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil {
    public class MultiplexAction : Action {
        public Collections.FastUnorderedList<Action> actions;

        public MultiplexAction Init(Action[] actions) {
            if (this.actions == null) {
                this.actions = new Collections.FastUnorderedList<Action>(actions.Length);
            }

            this.actions.InsertMany(actions);

            return this;
        }

        protected override bool Run() {
            for (int i = 0; i < actions.Count;) {
                if (actions[i].Execute()) {
                    actions.Remove(i);
                } else {
                    ++i;
                }
            }

            return actions.Count == 0; // Finish when all actions are done.
        }

        public override IEnumerable<Action> GetSubactions() =>
            actions;

        public override string ToString() =>
            string.Format("MultiplexAction (<color=#{0}>{1}</color> left)", ColorUtility.ToHtmlStringRGBA(master.Config.Debug.INFO_ACCENT_COLOR_PRIMARY), actions.Count);
    }
}
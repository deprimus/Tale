using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaleUtil {
    public class SceneAction : Action {
        enum Type {
            INDEX,
            PATH
        }

        string path;
        int index;

        Type type;

        public SceneAction Init(int index) {
            this.index = index;
            type = Type.INDEX;

            return this;
        }

        public SceneAction Init(string path) {
            this.path = path;
            type = Type.PATH;

            return this;
        }

        public override bool Run() {
            switch (type) {
                case Type.INDEX:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + index);
                    break;
                case Type.PATH:
                    SceneManager.LoadScene(path);
                    break;
            }

            return true;
        }

        public override string ToString() {
            string scene = null;

            switch (type) {
                case Type.INDEX: {
                    scene = index.ToString();
                    break;
                }
                case Type.PATH: {
                    scene = path;
                    break;
                }
            }

            return string.Format("SceneAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGB(master.config.Core.DEBUG_ACCENT_COLOR_SECONDARY), scene);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaleUtil
{
    public class SceneAction : TaleUtil.Action
    {
        private enum Type
        {
            INDEX,
            PATH
        }

        private string path;
        private int index;

        private Type type;

        private SceneAction() { }

        public SceneAction(int index)
        {
            this.index = index;
            type = Type.INDEX;   
        }

        public SceneAction(string path)
        {
            this.path = path;
            type = Type.PATH;
        }

        public override TaleUtil.Action Clone()
        {
            SceneAction clone = new SceneAction();
            clone.index = index;
            clone.path = path;
            clone.type = type;

            return clone;
        }

        public override bool Run()
        {
            switch(type)
            {
                case Type.INDEX:
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + index);
                    break;
                case Type.PATH:
                    SceneManager.LoadScene(path);
                    break;
            }

            return true;
        }
    }
}
using UnityEngine.SceneManagement;

namespace TaleUtil
{
    public class SceneAction : Action
    {
        enum Type
        {
            INDEX,
            PATH
        }

        string path;
        int index;

        Type type;

        SceneAction() { }

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

        public override Action Clone()
        {
            SceneAction clone = new SceneAction();
            clone.delta = delta;
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

        public override string ToString()
        {
            return "SceneAction";
        }
    }
}
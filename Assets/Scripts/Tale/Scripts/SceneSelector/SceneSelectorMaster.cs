using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace TaleUtil.Scripts
{
    public class SceneSelectorMaster : MonoBehaviour
    {
        public RectTransform sceneItemParent;

        public GameObject sceneItemPrefab;

        public SceneSelectorScrollbar scrollbar;

        void Awake()
        {
            var config = Tale.Master.Config;

            if (!config.SCENE_SELECTOR_ENABLE)
            {
                Log.Warning("Scene Selector", "Scene selector is disabled, but somehow we got here; loading the next scene");
                Tale.Scene();
                return;
            }

            var count = SceneManager.sceneCountInBuildSettings;

            var blacklist = config.SCENE_SELECTOR_BLACKLIST;

            for (int i = 0; i < count; ++i)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (path == SceneManager.GetActiveScene().path || (blacklist != null && blacklist.Contains(Path.NormalizeResourcePath(path))))
                {
                    continue; // Ignore scene selector + blacklisted
                }

                GameObject obj = Instantiate(sceneItemPrefab);
                obj.GetComponent<RectTransform>().SetParent(sceneItemParent, false);

                var item = obj.GetComponent<SceneSelectorItem>();

                item.Init(name, path, this, scrollbar);
            }
        }

        public void OnSelectScene(string scene)
        {
            enabled = false;

            Tale.TransitionOut("Fade", 0.5f);
            Tale.Wait(0.5f);
            Tale.Scene(scene);

            // Handled directly by each scene
            //Tale.TransitionIn(0.5f);
        }
    }
}
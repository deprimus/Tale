using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace TaleUtil.Scripts
{
    public class SceneSelectorMaster : MonoBehaviour
    {
        [SerializeField]
        RectTransform sceneItemParent;

        [SerializeField]
        GameObject sceneItemPrefab;

        [SerializeField]
        SceneSelectorScrollbar scrollbar;

        void Awake()
        {
            var count = SceneManager.sceneCountInBuildSettings;

            var blacklist = Tale.config.SCENE_SELECTOR_BLACKLIST;

            for (int i = 0; i < count; ++i)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (path == SceneManager.GetActiveScene().path || (blacklist != null && blacklist.Contains(Path.NormalizeAssetPath(path))))
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

            Tale.Transition("Fade", Tale.TransitionType.OUT, 0.5f);
            Tale.Wait(0.5f);
            Tale.Scene(scene);
            Tale.Transition("Fade", Tale.TransitionType.IN, 0.5f);
        }
    }
}
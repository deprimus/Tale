using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace TaleUtil.Scripts
{
    public class SceneSelectorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public SceneSelectorMaster master;
        public SceneSelectorScrollbar scrollbar;

        [SerializeField]
        RectTransform outlineTform;

        [SerializeField]
        Image outline;

        [SerializeField]
        new TextMeshProUGUI name;

        [SerializeField]
        Image thumbnail;

        string path;

        const float OUTLINE_NORMAL_OFFSET = 3f;
        const float OUTLINE_FOCUSED_OFFSET = 0f;

        readonly Color COLOR_ACTIVE = Color.white;
        readonly Color COLOR_INACTIVE = new Color(0.9f, 0.9f, 0.9f);

        public void Init(string name, string path, SceneSelectorMaster master, SceneSelectorScrollbar scrollbar)
        {
            this.name.text = name;
            this.path = path;
            this.master = master;
            this.scrollbar = scrollbar;

            var img = Resources.Load<Sprite>(Path.NormalizeAssetPath(SceneThumbnailGenerator.GetThumbnailPathForScenePath(path)));

            if (img != null)
            {
                thumbnail.sprite = img;
                thumbnail.color = COLOR_INACTIVE;
            }
            else
            {
                TaleUtil.Log.Warning("SceneSelector", string.Format("Scene '{0}' has no thumbnail; make one by loading the scene and pressing F11", name));
            }

            outline.color = COLOR_INACTIVE;
            this.name.color = COLOR_INACTIVE;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (master.enabled)
            {
                outlineTform.offsetMin = new Vector2(OUTLINE_FOCUSED_OFFSET, OUTLINE_FOCUSED_OFFSET);
                outlineTform.offsetMax = new Vector2(-OUTLINE_FOCUSED_OFFSET, -OUTLINE_FOCUSED_OFFSET);

                if (thumbnail.sprite != null)
                {
                    thumbnail.color = COLOR_ACTIVE;
                }

                outline.color = COLOR_ACTIVE;
                name.color = COLOR_ACTIVE;

                scrollbar.Show();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (master.enabled)
            {
                outlineTform.offsetMin = new Vector2(OUTLINE_NORMAL_OFFSET, OUTLINE_NORMAL_OFFSET);
                outlineTform.offsetMax = new Vector2(-OUTLINE_NORMAL_OFFSET, -OUTLINE_NORMAL_OFFSET);

                if (thumbnail.sprite != null)
                {
                    thumbnail.color = COLOR_INACTIVE;
                }

                outline.color = COLOR_INACTIVE;
                name.color = COLOR_INACTIVE;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (master.enabled)
            {
                master.OnSelectScene(path);
            }
        }
    }
}
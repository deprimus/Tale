using UnityEngine;

namespace TaleUtil.Scripts
{
    /*
     * This script should be attached to an object that has a Mask component and a child object.
     * When the mask object is moved, the child object will be held in place.
     */
    public class UIMaskFixer : MonoBehaviour
    {
        RectTransform tform;
        RectTransform child;

        Vector2 tformBasePos;
        Vector3 tformBaseScale;

        Vector2 childBasePos;
        Vector3 childBaseScale;

        void Awake()
        {
            tform = GetComponent<RectTransform>();
            child = tform.GetChild(0).GetComponent<RectTransform>();

            UpdateBaseValues();
        }

        void Update()
        {
            if (tform.hasChanged)
            {
                child.anchoredPosition = childBasePos - (tform.anchoredPosition - tformBasePos);
                child.localScale = childBaseScale.Div2D(tform.localScale.Div2D(tformBaseScale));

                child.anchoredPosition = child.anchoredPosition * child.localScale;

                UpdateBaseValues();

                tform.hasChanged = false;
            }
        }

        void UpdateBaseValues()
        {
            tformBasePos = tform.anchoredPosition;
            tformBaseScale = tform.localScale;

            childBasePos = child.anchoredPosition;
            childBaseScale = child.localScale;
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace TaleUtil.Scripts.Choice.Default {
    public class ChoiceObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
        RectTransform tform;
        TextMeshProUGUI text;

        TaleUtil.Delegates.ShallowDelegate action;

        void Awake() {
            tform = GetComponent<RectTransform>();
            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        internal void Present(string choice, TaleUtil.Delegates.ShallowDelegate onChoice) {
            text.text = choice;
            action = onChoice;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            text.color = Color.yellow;
        }

        public void OnPointerExit(PointerEventData eventData) {
            text.color = Color.white;
        }

        public void OnPointerClick(PointerEventData eventData) {
            action();
        }
    }
}
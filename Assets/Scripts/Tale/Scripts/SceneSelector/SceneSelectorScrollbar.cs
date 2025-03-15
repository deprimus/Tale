using UnityEngine;
using UnityEngine.UI;

namespace TaleUtil.Scripts
{
    public class SceneSelectorScrollbar : MonoBehaviour
    {
        private enum State
        {
            SHOWN,
            HIDING,
            HIDDEN
        }

        public Image handleImage;

        float DECAY_DELAY = 2f;
        float DECAY_DURATION = 1f;

        float clock;
        bool hidden;

        float lastValue;

        State state = State.HIDDEN;

        void Start()
        {
            Hide();

            var scrollbar = GetComponent<Scrollbar>();

            scrollbar.value = 1f;
            lastValue = scrollbar.value;

            scrollbar.onValueChanged.AddListener((value) => {
                // Prevent noise
                if (Mathf.Abs(value - lastValue) > 0.001f)
                {
                    Show();
                }

                lastValue = value;
            });
        }

        void Update()
        {
            switch (state)
            {
                case State.SHOWN:
                    clock += Time.unscaledDeltaTime;

                    if (clock >= DECAY_DELAY)
                    {
                        clock = 0f;
                        state = State.HIDING;
                        return;
                    }
                    break;
                case State.HIDING:
                    clock += Time.unscaledDeltaTime;

                    if (clock >= DECAY_DURATION)
                    {
                        Hide();
                    }
                    else
                    {
                        SetAlpha(TaleUtil.Math.Map(clock, 0, DECAY_DURATION, 1f, 0f));
                    }
                    break;
                default:
                    return;
            }
        }

        void SetAlpha(float value)
        {
            handleImage.color = new Color(1f, 1f, 1f, value);
        }

        public void Hide()
        {
            SetAlpha(0f);
            clock = 0f;
            state = State.HIDDEN;
        }

        public void Show()
        {
            SetAlpha(1f);
            clock = 0f;
            state = State.SHOWN;
        }
    }
}
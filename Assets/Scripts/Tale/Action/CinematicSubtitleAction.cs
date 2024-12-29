using UnityEngine;
using TMPro;

namespace TaleUtil
{
    public class CinematicSubtitleAction : Action
    {
        enum State
        {
            SETUP,
            WAIT
        }

        struct RectBounds
        {
            public float left;
            public float right;
            public float top;
            public float bottom;

            public RectBounds(float l, float r, float t, float b)
            {
                left = l;
                right = r;
                top = t;
                bottom = b;
            }
        }

        string content;
        float ttl;
        bool showBackground;

        State state;

        float clock;

        CinematicSubtitleAction() { }

        public CinematicSubtitleAction(string content, float ttl, bool showBackground)
        {
            Assert.Condition(Props.cinematic.subtitlesGroup != null, "CinematicSubtitleAction requires a subtitles group object; did you forget to register it in TaleMaster?");
            Assert.Condition(Props.cinematic.subtitles != null, "CinematicSubtitleAction requires a subtitles object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            this.content = content;
            this.ttl = ttl;
            this.showBackground = showBackground;

            if(this.showBackground)
                Assert.Condition(Props.cinematic.subtitlesBackground != null, "CinematicSubtitleAction with 'show background' requires a subtitle background object with a RectTransform component; did you forget to register it in TaleMaster?");

            state = State.SETUP;

            clock = 0f;
        }

        public override Action Clone()
        {
            CinematicSubtitleAction clone = new CinematicSubtitleAction();
            clone.delta = delta;
            clone.content = content;
            clone.ttl = ttl;
            clone.showBackground = showBackground;
            clone.state = state;
            clone.clock = clock;

            return clone;
        }

        public override bool Run()
        {
            switch (state)
            {
                case State.SETUP:
                {
                    Props.cinematic.subtitles.text = content;

                    if(!Props.cinematic.subtitlesGroup.activeSelf)
                        Props.cinematic.subtitlesGroup.SetActive(true);

                    // This forces TMP to update the characterInfo field (and others). Note that it only works when the subtitlesGroup is active.
                    Props.cinematic.subtitles.ForceMeshUpdate();

                    // Rescale subtitles background.
                    if (showBackground)
                    {
                        // Note: If the new text is shorter, characterInfo MAY not change size. This is why characterCount is used instead of info.Length
                        //       This way of rescaling the background only works when the TextMeshProUGUI object has the horizontal alignment set to "Middle" and the vertical one to "Top".
                        int length = Props.cinematic.subtitles.textInfo.characterCount;
                        TMP_CharacterInfo[] info = Props.cinematic.subtitles.textInfo.characterInfo;

                        if(length > 0)
                        {
                            RectBounds bounds = new RectBounds(info[0].topLeft.x, info[0].bottomRight.x, info[0].topLeft.y, info[0].bottomRight.y);

                            for(int i = 1; i < length; ++i)
                            {
                                if(info[i].isVisible) {
                                    if(info[i].topLeft.x < bounds.left)
                                        bounds.left = info[i].topLeft.x;
                                    if(info[i].topLeft.y > bounds.top)
                                        bounds.top = info[i].topLeft.y;
                                    if(info[i].bottomRight.x > bounds.right)
                                        bounds.right = info[i].bottomRight.x;
                                    if(info[i].bottomRight.y < bounds.bottom)
                                        bounds.bottom = info[i].bottomRight.y;
                                }
                            }

                            bounds.left   -= Tale.config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING.x;
                            bounds.right  += Tale.config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING.x;
                            bounds.top    += Tale.config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING.y;
                            bounds.bottom -= Tale.config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING.y;

                            float width = Mathf.Abs(bounds.right - bounds.left);
                            float height = Mathf.Abs(bounds.top - bounds.bottom);

                            Props.cinematic.subtitlesBackground.sizeDelta = new Vector2(width, height);

                            // Dynamically center the background based on the bounds.
                            Props.cinematic.subtitlesBackground.anchoredPosition = new Vector2(Props.cinematic.subtitles.rectTransform.anchoredPosition.x + bounds.left + width / 2, Props.cinematic.subtitles.rectTransform.anchoredPosition.y + bounds.bottom + height / 2);
                        }
                    }

                    state = State.WAIT;

                    break;
                }
                case State.WAIT:
                {
                    clock += delta();

                    if(clock >= ttl)
                    {
                        if(!(Queue.FetchNext() is CinematicSubtitleAction))
                        {
                            Props.cinematic.subtitlesGroup.SetActive(false);
                        }

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("CinematicSubtitleAction ({0})", state.ToString());
        }
    }
}
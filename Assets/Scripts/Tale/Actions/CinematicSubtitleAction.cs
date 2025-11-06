using UnityEngine;
using TMPro;

namespace TaleUtil {
    public class CinematicSubtitleAction : Action {
        enum State {
            SETUP,
            WAIT
        }

        struct RectBounds {
            public float left;
            public float right;
            public float top;
            public float bottom;

            public RectBounds(float l, float r, float t, float b) {
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

        public CinematicSubtitleAction Init(string content, float ttl, bool showBackground) {
            Assert.Condition(master.Props.cinematic.subtitlesGroup != null, "CinematicSubtitleAction requires a subtitles group object; did you forget to register it in TaleMaster?");
            Assert.Condition(master.Props.cinematic.subtitles != null, "CinematicSubtitleAction requires a subtitles object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            this.content = content;
            this.ttl = ttl;
            this.showBackground = showBackground;

            if (this.showBackground) {
                Assert.Condition(master.Props.cinematic.subtitlesBackground != null, "CinematicSubtitleAction with 'show background' requires a subtitle background object with a RectTransform component; did you forget to register it in TaleMaster?");
            }

            state = State.SETUP;

            clock = 0f;

            return this;
        }

        protected override bool Run() {
            switch (state) {
                case State.SETUP: {
                    master.Props.cinematic.subtitles.text = content;

                    if (!master.Props.cinematic.subtitlesGroup.activeSelf)
                        master.Props.cinematic.subtitlesGroup.SetActive(true);

                    // This forces TMP to update the characterInfo field (and others). Note that it only works when the subtitlesGroup is active.
                    master.Props.cinematic.subtitles.ForceMeshUpdate();

                    // Rescale subtitles background.
                    if (showBackground) {
                        // Note: If the new text is shorter, characterInfo MAY not change size. This is why characterCount is used instead of info.Length
                        //       This way of rescaling the background only works when the TextMeshProUGUI object has the horizontal alignment set to "Middle" and the vertical one to "Top".
                        int length = master.Props.cinematic.subtitles.textInfo.characterCount;
                        TMP_CharacterInfo[] info = master.Props.cinematic.subtitles.textInfo.characterInfo;

                        if (length > 0) {
                            RectBounds bounds = new RectBounds(info[0].topLeft.x, info[0].bottomRight.x, info[0].topLeft.y, info[0].bottomRight.y);

                            for (int i = 1; i < length; ++i) {
                                if (info[i].isVisible) {
                                    if (info[i].topLeft.x < bounds.left)
                                        bounds.left = info[i].topLeft.x;
                                    if (info[i].topLeft.y > bounds.top)
                                        bounds.top = info[i].topLeft.y;
                                    if (info[i].bottomRight.x > bounds.right)
                                        bounds.right = info[i].bottomRight.x;
                                    if (info[i].bottomRight.y < bounds.bottom)
                                        bounds.bottom = info[i].bottomRight.y;
                                }
                            }

                            bounds.left -= master.Config.Cinematic.SUBTITLE_BACKGROUND_PADDING.x;
                            bounds.right += master.Config.Cinematic.SUBTITLE_BACKGROUND_PADDING.x;
                            bounds.top += master.Config.Cinematic.SUBTITLE_BACKGROUND_PADDING.y;
                            bounds.bottom -= master.Config.Cinematic.SUBTITLE_BACKGROUND_PADDING.y;

                            float width = Mathf.Abs(bounds.right - bounds.left);
                            float height = Mathf.Abs(bounds.top - bounds.bottom);

                            master.Props.cinematic.subtitlesBackground.sizeDelta = new Vector2(width, height);

                            // Dynamically center the background based on the bounds.
                            master.Props.cinematic.subtitlesBackground.anchoredPosition = new Vector2(master.Props.cinematic.subtitles.rectTransform.anchoredPosition.x + bounds.left + width / 2, master.Props.cinematic.subtitles.rectTransform.anchoredPosition.y + bounds.bottom + height / 2);
                        }
                    }

                    state = State.WAIT;

                    break;
                }
                case State.WAIT: {
                    clock += delta();

                    if (clock >= ttl) {
                        if (!(Tale.Master.Queue.FetchNext() is CinematicSubtitleAction)) {
                            master.Props.cinematic.subtitlesGroup.SetActive(false);
                        }

                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        public override string ToString() {
            return string.Format("CinematicSubtitleAction (<color=#{0}>{1}</color>)", ColorUtility.ToHtmlStringRGBA(master.config.Core.DEBUG_ACCENT_COLOR_PRIMARY), state.ToString());
        }
    }
}
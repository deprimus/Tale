#pragma warning disable 0162 // Disable the 'unreachable code' warning caused by config constants.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TaleUtil
{
    public class CinematicSubtitleAction : TaleUtil.Action
    {
        private enum State
        {
            SETUP,
            WAIT
        }

        private struct RectBounds
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

        private string content;
        private float ttl;
        private bool showBackground;

        private State state;

        private float clock;

        private CinematicSubtitleAction() { }

        public CinematicSubtitleAction(string content, float ttl, bool showBackground)
        {
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.subtitlesGroup, "CinematicSubtitleAction requires a subtitles group object; did you forget to register it in TaleMaster?");
            TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.subtitles, "CinematicSubtitleAction requires a subtitles object with a TextMeshProUGUI component; did you forget to register it in TaleMaster?");

            this.content = content;
            this.ttl = ttl;
            this.showBackground = showBackground;

            if(this.showBackground)
                TaleUtil.Assert.NotNull(TaleUtil.Props.cinematic.subtitlesBackground, "CinematicSubtitleAction with 'show background' requires a subtitle background object with a RectTransform component; did you forget to register it in TaleMaster?");

            state = State.SETUP;

            clock = 0f;
        }

        public override TaleUtil.Action Clone()
        {
            CinematicSubtitleAction clone = new CinematicSubtitleAction();
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
                    TaleUtil.Props.cinematic.subtitles.text = content;

                    if(!TaleUtil.Props.cinematic.subtitlesGroup.activeSelf)
                        TaleUtil.Props.cinematic.subtitlesGroup.SetActive(true);

                    // This forces TMP to update the characterInfo field (and others). Note that it only works when the subtitlesGroup is active.
                    TaleUtil.Props.cinematic.subtitles.ForceMeshUpdate();

                    // Rescale subtitles background.
                    if (showBackground)
                    {
                        // Note: If the new text is shorter, characterInfo MAY not change size. This is why characterCount is used instead of info.Length
                        //       This way of rescaling the background only works when the TextMeshProUGUI object has both the horizontal and vertical alignments set to "Middle".
                        int length = TaleUtil.Props.cinematic.subtitles.textInfo.characterCount;
                        TMP_CharacterInfo[] info = TaleUtil.Props.cinematic.subtitles.textInfo.characterInfo;

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

                            bounds.left   -= TaleUtil.Config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING_X;
                            bounds.right  += TaleUtil.Config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING_X;
                            bounds.top    += TaleUtil.Config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING_Y;
                            bounds.bottom -= TaleUtil.Config.CINEMATIC_SUBTITLE_BACKGROUND_PADDING_Y;

                            float width = Mathf.Abs(bounds.right - bounds.left);
                            float height = Mathf.Abs(bounds.top - bounds.bottom);

                            TaleUtil.Props.cinematic.subtitlesBackground.sizeDelta = new Vector2(width, height);

                            // In case this is ever needed to dynamically center the background based on the bounds.
                            //TaleUtil.Props.cinematic.subtitlesBackground.anchoredPosition = new Vector2(TaleUtil.Props.cinematic.subtitles.rectTransform.anchoredPosition.x + bounds.left + width / 2, TaleUtil.Props.cinematic.subtitles.rectTransform.anchoredPosition.y + bounds.bottom + height / 2);
                        }
                    }

                    state = State.WAIT;

                    break;
                }
                case State.WAIT:
                {
                    clock += Time.deltaTime;

                    if(clock >= ttl)
                    {
                        if(!(TaleUtil.Queue.FetchNext() is CinematicSubtitleAction))
                        {
                            TaleUtil.Props.cinematic.subtitlesGroup.SetActive(false);
                        }

                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}
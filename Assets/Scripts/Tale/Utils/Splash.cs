using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TaleUtil.Scripts {
    public class Splash : MonoBehaviour {
        public List<string> soundVariants;

        public string transition = "fade";
        public float transitionDelay = 1.5f;

        public GameObject curtain;

        void Awake() {
            // Since we don't know if previous scenes used other transitions,
            // we show a black screen ('curtain') to ensure the logo isn't visible.
            // After we make sure that only our transition's canvas is active, we hide the curtain.
            Tale.TransitionIn(0f);
            Tale.TransitionOut(transition, 0f);

            Tale.Exec(() => curtain.SetActive(false));

            Tale.Wait();

            if (soundVariants != null && soundVariants.Count > 0) {
                string sound = soundVariants[Random.Range(0, soundVariants.Count)];
                Tale.Sound.Play(sound);
            }

            Tale.TransitionIn(0.75f);
            Tale.Wait(transitionDelay);
            Tale.TransitionOut(transition, 0.75f);

            Tale.Scene();
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Splash : MonoBehaviour
{
    public List<string> soundVariants;

    public float fadeDelay = 1.5f;

    void Awake()
    {
        Tale.Wait();

        if (soundVariants != null && soundVariants.Count > 0)
        {
            string sound = soundVariants[Random.Range(0, soundVariants.Count)];
            Tale.Sound.Play(sound);
        }

        Tale.Transition("fade", Tale.TransitionType.IN, 0.75f);
        Tale.Wait(fadeDelay);
        Tale.Transition("fade", Tale.TransitionType.OUT, 0.75f);

        Tale.Scene();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Splash : MonoBehaviour
{
    public List<AudioClip> soundVariants;

    public float fadeDelay = 1.5f;

    void Awake()
    {
        Tale.Transition("fade", Tale.TransitionType.OUT, 0f);

        Tale.Wait();

        if (soundVariants != null && soundVariants.Count > 0)
        {
            AudioClip sound = soundVariants[Random.Range(0, soundVariants.Count)];
            Tale.Sound.Play(AssetDatabase.GetAssetPath(sound));
        }

        Tale.Transition("fade", Tale.TransitionType.IN, 0.75f);
        Tale.Wait(fadeDelay);
        Tale.Transition("fade", Tale.TransitionType.OUT, 0.75f);

        Tale.Scene();
    }
}

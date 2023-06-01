using UnityEngine;

public class Splash : MonoBehaviour
{
    public string soundPath;

    public float fadeDelay = 1.5f;

    void Awake()
    {
        Tale.Transition("fade", Tale.TransitionType.OUT, 0f);

        Tale.Wait();

        Tale.Sound.Play(soundPath);
        Tale.Transition("fade", Tale.TransitionType.IN, 0.75f);
        Tale.Wait(fadeDelay);
        Tale.Transition("fade", Tale.TransitionType.OUT, 0.75f);

        Tale.Scene();
    }
}

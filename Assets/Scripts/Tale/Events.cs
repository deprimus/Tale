using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

namespace TaleUtil
{
    public static class Events
    {
        public static void OnCinematicVideoEnd(VideoPlayer player)
        {
            player.Stop();
            player.targetTexture.Release(); // The RenderTexture holds the last frame from the last video. This clears it.
            TaleUtil.Props.cinematic.video.group.SetActive(false);
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TaleUtil.Props.ReinitCamera();
        }
    }
}
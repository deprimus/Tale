using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaleUtil
{
    public class SceneThumbnailGenerator : MonoBehaviour
    {
        public static string GetThumbnailPathForScenePath(string path)
        {
            string filename = path.Replace('/', '_');
            filename = System.IO.Path.GetFileNameWithoutExtension(filename);

            return System.IO.Path.Combine("Assets/Resources", Config.Setup.ASSET_ROOT_SCENE_THUMBNAIL, filename).Replace('\\', '/');
        }

#if UNITY_EDITOR
        static GameObject temp;

        // ScreenCapture.CaptureScreenshot doesn't support resolutions lower than the current game view.
        // Therefore, capture to a RenderTexture and resize.
        // However, this requires the current frame to be fully rendered.
        // The only way to do this is (unfortunately) via coroutines and WaitForEndOfFrame.

        public static Task CaptureThumbnail()
        {
            var task = new TaskCompletionSource<bool>();

            // Focus on Game View
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(type);

            // Prepare to capture
            temp = new GameObject("_TaleSceneThumbnailGenerator");
            temp.AddComponent<SceneThumbnailGenerator>().StartCoroutine(CaptureCoroutine(task));

            return task.Task;
        }

        static IEnumerator CaptureCoroutine(TaskCompletionSource<bool> task)
        {
            // Wait until the frame is finished rendering.
            // Otherwise, the thumbnail will contain random stuff like the editor background.
            // Unfortunately, there isn't a better way to do this, so we must use coroutines
            yield return new WaitForEndOfFrame();

            string path = GetThumbnailPathForScenePath(SceneManager.GetActiveScene().path);

            int width = Config.Setup.SCENE_THUMBNAIL_WIDTH;
            int height = Config.Setup.SCENE_THUMBNAIL_HEIGHT;

            RenderTexture captured = new RenderTexture(Screen.width, Screen.height, 24);

            ScreenCapture.CaptureScreenshotIntoRenderTexture(captured);

            RenderTexture resized = new RenderTexture(width, height, 24);

            float scaleY = 1;
            float offsetY = 0f;

            // OpenGL and Vulkan store textures normally, while others store them upside down.
            // This ensures that thumbnails are always generated correctly.
            switch (SystemInfo.graphicsDeviceType)
            {
                case GraphicsDeviceType.OpenGLCore:
                case GraphicsDeviceType.OpenGLES2:
                case GraphicsDeviceType.OpenGLES3:
                case GraphicsDeviceType.Vulkan:
                    break;
                default:
                {
                    scaleY = -1f;
                    offsetY = 1f;
                    break;
                }
            }

            resized.filterMode = FilterMode.Point;

            Graphics.Blit(captured, resized, new Vector2(1f, scaleY), new Vector2(0f, offsetY));

            var backup = RenderTexture.active;
            RenderTexture.active = resized;

            Texture2D texture = new Texture2D(resized.width, resized.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, resized.width, resized.height), 0, 0);
            texture.Apply();

            RenderTexture.active = backup;

            File.WriteAllBytes(path + ".png", texture.EncodeToPNG());

            DestroyImmediate(texture);
            resized.Release();
            DestroyImmediate(resized);
            captured.Release();
            DestroyImmediate(captured);

            DestroyImmediate(temp);
            temp = null;

            Debug.Log("Generated thumbnail for scene " + SceneManager.GetActiveScene().path);

            task.SetResult(true);
        }
#endif
    }
}
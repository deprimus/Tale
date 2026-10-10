#if UNITY_EDITOR && UNITY_6000_3_OR_NEWER
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Toolbars;

namespace TaleUtil
{
    static class ToolbarLogo
    {
        const string ID = "Tale/Logo";
        const string LOGO_PATH = "Assets/Resources/Tale/Logo.png";
        const float LOGO_HEIGHT = 18f;
        const int FIT_MAX_TRIES = 100;

        static Texture2D logo;
        static int fitTries;

        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Left, defaultDockIndex = 0)]
        static MainToolbarElement CreateLogo()
        {
            if (logo == null)
            {
                logo = CreateSmallLogo(Mathf.RoundToInt(LOGO_HEIGHT * EditorGUIUtility.pixelsPerPoint));
            }

            fitTries = 0;
            EditorApplication.update -= FitLogo;
            EditorApplication.update += FitLogo;

            return new MainToolbarLabel(new MainToolbarContent(logo, "Tale"));
        }

        static void FitLogo()
        {
            if (++fitTries > FIT_MAX_TRIES)
            {
                EditorApplication.update -= FitLogo;
                return;
            }

            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window.GetType().Name != "MainToolbarWindow")
                {
                    continue;
                }

                VisualElement element = window.rootVisualElement.Q(ID);
                Image icon = element?.Q<Image>(className: "unity-editor-toolbar-element__icon");

                if (icon == null || icon.image != logo)
                {
                    continue;
                }

                icon.style.height = LOGO_HEIGHT;
                icon.style.width = LOGO_HEIGHT * logo.width / logo.height;
                EditorApplication.update -= FitLogo;
            }
        }

        static Texture2D CreateSmallLogo(int height)
        {
            Texture2D source = new Texture2D(2, 2);
            source.LoadImage(File.ReadAllBytes(LOGO_PATH));

            int srcWidth = source.width;
            int srcHeight = source.height;
            int width = Mathf.Max(1, Mathf.RoundToInt((float)height * srcWidth / srcHeight));

            Color32[] src = source.GetPixels32();
            Color32[] dst = new Color32[width * height];

            Object.DestroyImmediate(source);

            for (int y = 0; y < height; ++y)
            {
                int y0 = y * srcHeight / height;
                int y1 = Mathf.Max(y0 + 1, (y + 1) * srcHeight / height);

                for (int x = 0; x < width; ++x)
                {
                    int x0 = x * srcWidth / width;
                    int x1 = Mathf.Max(x0 + 1, (x + 1) * srcWidth / width);

                    float r = 0f, g = 0f, b = 0f, a = 0f;

                    for (int sy = y0; sy < y1; ++sy)
                    {
                        for (int sx = x0; sx < x1; ++sx)
                        {
                            Color32 c = src[sy * srcWidth + sx];
                            float alpha = c.a / 255f;

                            r += c.r * alpha;
                            g += c.g * alpha;
                            b += c.b * alpha;
                            a += alpha;
                        }
                    }

                    int count = (x1 - x0) * (y1 - y0);

                    if (a > 0f)
                    {
                        dst[y * width + x] = new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), (byte)(a / count * 255f));
                    }
                }
            }

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.name = "TaleToolbarLogo";
            result.hideFlags = HideFlags.HideAndDontSave;
            result.filterMode = FilterMode.Bilinear;
            result.wrapMode = TextureWrapMode.Clamp;
            result.SetPixels32(dst);
            result.Apply(false, true);

            return result;
        }
    }
}
#endif

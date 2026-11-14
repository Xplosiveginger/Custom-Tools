using UnityEngine;

namespace XG
{
    public static class Helper
    {
        public static Texture2D MakeRoundedTexture(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // Distance from the nearest corner circle's center (only matters inside corner zones)
                    float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f); // anti-aliased edge
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            tex.Apply();
            return tex;
        }

        public static GUIStyle GetPillStyle()
        {
            Texture2D roundedTex = MakeRoundedTexture(16, 6);

            GUIStyle pillStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,                              // smaller text
                fontStyle = FontStyle.Bold,
                border = new RectOffset(6, 6, 6, 6),       // matches the new radius
                padding = new RectOffset(6, 6, 0, 0),      // tighter padding
                margin = new RectOffset(0, 0, 0, 0),
                stretchWidth = false,                      // don't expand horizontally
                stretchHeight = true
            };
            pillStyle.normal.background = roundedTex;
            pillStyle.normal.textColor = Color.white;
            return pillStyle;
        }
    }
}
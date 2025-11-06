using UnityEngine;
using System.Text;

[DisallowMultipleComponent]
public class CooldownUI : MonoBehaviour
{
    public Font uiFont;
    public int fontSize = 18;
    public Vector2 margin = new Vector2(40, 40);
    public Color textColor = Color.white;
    public Color bgColor = new Color(0f, 0f, 0f, 0.35f);

    private GUIStyle style;
    private Texture2D bgTex;

    private HeroKnight player;

    void Awake()
    {
        style = new GUIStyle { font = uiFont, fontSize = fontSize, normal = { textColor = textColor } };
        bgTex = new Texture2D(1, 1); bgTex.SetPixel(0, 0, bgColor); bgTex.Apply();

        var p = GameObject.Find("HeroKnight");
        if (p) player = p.GetComponent<HeroKnight>();
    }

    void OnGUI()
    {
        if (player == null) return;

        var rect = new Rect(margin.x, Screen.height - margin.y - 50, 240, 40);
        GUI.DrawTexture(rect, bgTex);

        float pcd = player.ParryCooldownRemaining;

        var sb = new StringBuilder();
        sb.Append($"Parry CD : {(pcd > 0f ? pcd.ToString("0.0") + "s" : "Ready")}");

        GUI.Label(new Rect(rect.x + 14, rect.y + 10, rect.width - 20, rect.height - 20), sb.ToString(), style);
    }
}

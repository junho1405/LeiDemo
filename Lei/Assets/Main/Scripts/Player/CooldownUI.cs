using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 화면 좌하단에 쿨타임/락/경직 시간을 표기하는 간단 HUD.
/// - 아무 오브젝트에 붙여도 됨(권장: 빈 GameObject "HUD").
/// - 첫 생성 씬에서만 하나 두고, DontDestroyOnLoad로 유지.
/// - HeroKnight는 씬 전환마다 재탐색.
/// </summary>
[DisallowMultipleComponent]
public class CooldownUI : MonoBehaviour
{
    [Header("Font / UI Look")]
    public Font uiFont;                  // 비워도 동작함(디폴트 폰트)
    public int fontSize = 18;
    public Vector2 margin = new Vector2(16, 16); // 좌하단 여백
    public Color textColor = Color.white;
    public Color bgColor = new Color(0, 0, 0, 0.35f);

    private Canvas canvas;
    private RectTransform panel;
    private Text text;
    private HeroKnight hero;

    void Awake()
    {
        // 싱글톤 느낌으로 중복 방지(선택)
        var existing = FindObjectsByType<CooldownUI>(FindObjectsSortMode.None);
        if (existing != null && existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Canvas 생성 (Screen Space - Overlay)
        canvas = new GameObject("CooldownHUD_Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvas.gameObject);

        // Panel (배경)
        var panelGO = new GameObject("CooldownHUD_Panel");
        panel = panelGO.AddComponent<RectTransform>();
        panel.SetParent(canvas.transform, false);

        var img = panelGO.AddComponent<Image>();
        img.color = bgColor;

        // 좌하단 앵커 & 패딩
        panel.anchorMin = new Vector2(0, 0);
        panel.anchorMax = new Vector2(0, 0);
        panel.pivot = new Vector2(0, 0);
        panel.anchoredPosition = margin;
        panel.sizeDelta = new Vector2(320, 88); // 필요시 조정

        // Text
        var textGO = new GameObject("CooldownHUD_Text");
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.SetParent(panel, false);
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(8, 8);
        textRT.offsetMax = new Vector2(-8, -8);

        text = textGO.AddComponent<Text>();
        text.color = textColor;
        text.alignment = TextAnchor.LowerLeft;
        text.fontSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 씬 로드시 Hero 재탐색
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 최초 탐색
        FindHero();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindHero();
    }

    private void FindHero()
    {
        hero = FindFirstObjectByType<HeroKnight>();
        // 없다면 다음 프레임들에서 다시 Try (Update에서 null 체크)
    }

    void Update()
    {
        if (hero == null)
        {
            // 주기적으로 재탐색 (씬 전환 직후 대비)
            if (Time.frameCount % 30 == 0) FindHero();
            text.text = "Parry CD: -\nLock: -\nStun: -";
            return;
        }

        // 남은 시간 표시(소수 2자리)
        float cd = hero.ParryCooldownRemaining;
        float lockT = hero.AttackParryLockRemaining;
        float stun = hero.StunnedRemaining;

        text.text =
            $"Parry CD : {Format(cd)}\n" +
            $"Lock     : {Format(lockT)}\n" +
            $"Stun     : {Format(stun)}";
    }

    private string Format(float t)
    {
        if (t <= 0f) return "Ready";
        // 99.99 단위로 표기
        if (t >= 100f) return $"{Mathf.CeilToInt(t)}s";
        return $"{t:0.00}s";
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("HP Bar Layout")]
    public float hpWidth = 1.6f;
    public float hpHeight = 0.18f;
    public float yOffset = 0.8f;              // 머리 위로 띄우는 정도

    [Header("HP Colors")]
    public Color hpBgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color hpFillColor = new Color(0.2f, 0.85f, 0.2f, 0.95f);
    public Color hpLowColor = new Color(0.95f, 0.2f, 0.2f, 0.95f); // 30% 이하

    [Header("Stagger Bar Layout")]
    public bool showStaggerIfAvailable = true;
    public float staggerGap = 0.07f;          // HP바 아래로 간격
    public float staggerWidth = 1.6f;
    public float staggerHeight = 0.12f;

    [Header("Stagger Colors")]
    public Color staggerBgColor = new Color(0f, 0f, 0f, 0.5f);
    public Color staggerFillColor = new Color(0.95f, 0.8f, 0.2f, 0.95f);

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;     // 적 스프라이트보다 위로

    private SpriteRenderer _hpBg, _hpFill;
    private SpriteRenderer _sgBg, _sgFill; // stagger
    private EnemyBase _enemy;
    private Transform _root;
    private Sprite _pixel; // 1x1 스프라이트

    void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        if (_enemy == null)
        {
            Debug.LogWarning("[EnemyHealthBar] EnemyBase가 필요합니다.");
            enabled = false;
            return;
        }

        // 1x1 흰색 텍스처로 스프라이트 생성
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);

        // 루트
        _root = new GameObject("Bars").transform;
        _root.SetParent(transform, false);

        // HP BG
        _hpBg = NewSprite("HP_BG", hpBgColor, sortingOrder);
        // HP Fill
        _hpFill = NewSprite("HP_Fill", hpFillColor, sortingOrder + 1);

        // Stagger BG/Fill (일단 만들어두고 표시를 토글)
        _sgBg = NewSprite("ST_BG", staggerBgColor, sortingOrder);
        _sgFill = NewSprite("ST_Fill", staggerFillColor, sortingOrder + 1);

        LayoutAll();
        UpdateBarsImmediate();
    }

    SpriteRenderer NewSprite(string name, Color color, int order)
    {
        var sr = new GameObject(name).AddComponent<SpriteRenderer>();
        sr.transform.SetParent(_root, false);
        sr.sprite = _pixel;
        sr.color = color;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = order;
        return sr;
    }

    void LateUpdate()
    {
        if (_enemy == null) return;
        LayoutAll();
        UpdateBarsImmediate();
    }

    void LayoutAll()
    {
        // 기준 높이(머리 위)
        float top = transform.position.y;
        var col = GetComponent<Collider2D>();
        if (col != null) top = col.bounds.max.y;
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) top = sr.bounds.max.y;
        }
        _root.position = new Vector3(transform.position.x, top + yOffset, 0f);

        // HP 크기/위치
        _hpBg.transform.localScale = new Vector3(hpWidth, hpHeight, 1f);
        _hpFill.transform.localScale = new Vector3(hpWidth, hpHeight, 1f);
        _hpBg.transform.localPosition = Vector3.zero;
        _hpFill.transform.localPosition = Vector3.zero;

        // Stagger 크기/위치 (HP 아래로)
        bool showStagger = showStaggerIfAvailable && _enemy.HasStagger && _enemy.MaxStagger > 0.01f;
        _sgBg.gameObject.SetActive(showStagger);
        _sgFill.gameObject.SetActive(showStagger);

        if (showStagger)
        {
            Vector3 below = new Vector3(0f, -staggerGap - (hpHeight * 0.5f) - (staggerHeight * 0.5f), 0f);
            _sgBg.transform.localScale = new Vector3(staggerWidth, staggerHeight, 1f);
            _sgFill.transform.localScale = new Vector3(staggerWidth, staggerHeight, 1f);
            _sgBg.transform.localPosition = below;
            _sgFill.transform.localPosition = below;
        }
    }

    void UpdateBarsImmediate()
    {
        // ----- HP -----
        float hpMax = Mathf.Max(1, _enemy.MaxHP);
        float hpRatio = Mathf.Clamp01(_enemy.CurrentHP / hpMax);

        _hpFill.color = (hpRatio <= 0.3f) ? hpLowColor : hpFillColor;

        float hpFull = hpWidth;
        float hpCur = Mathf.Max(0f, hpFull * hpRatio);
        _hpFill.transform.localScale = new Vector3(hpCur, hpHeight, 1f);
        _hpFill.transform.localPosition = new Vector3((hpCur - hpFull) * 0.5f, 0f, 0f);

        // ----- STAGGER -----
        bool showStagger = showStaggerIfAvailable && _enemy.HasStagger && _enemy.MaxStagger > 0.01f;
        if (showStagger)
        {
            float sgMax = Mathf.Max(0.01f, _enemy.MaxStagger);
            float sgRatio = Mathf.Clamp01(_enemy.CurrentStagger / sgMax);

            float sgFull = staggerWidth;
            float sgCur = Mathf.Max(0f, sgFull * sgRatio);
            _sgFill.transform.localScale = new Vector3(sgCur, staggerHeight, 1f);
            _sgFill.transform.localPosition = new Vector3((sgCur - sgFull) * 0.5f, _sgFill.transform.localPosition.y, 0f);
        }

        // 사망시 제거
        if (_enemy.CurrentHP <= 0 && _root != null)
        {
            Destroy(_root.gameObject);
            enabled = false;
        }
    }
}

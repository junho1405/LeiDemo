using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Layout")]
    public float width = 3f;
    public float height = 0.3f;
    public float yOffset = 2f;

    [Header("Colors")]
    public Color bgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color fillHigh = new Color(0.20f, 0.85f, 0.20f, 0.95f);
    public Color fillLow = new Color(0.95f, 0.20f, 0.20f, 0.95f);
    public float lowHpThreshold = 0.30f;

    [Header("Sorting")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;

    Transform root;
    SpriteRenderer srBG;
    SpriteRenderer srFill;
    Sprite pxMid;
    Sprite pxLeft;

    EnemyBase enemy;
    SpriteRenderer enemySR;
    Collider2D enemyCol;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>() ?? GetComponentInParent<EnemyBase>() ?? GetComponentInChildren<EnemyBase>();
        enemySR = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        enemyCol = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();

        if (enemySR != null) { sortingLayerName = enemySR.sortingLayerName; sortingOrder = enemySR.sortingOrder + 10; }

        // ★ FullRect로 생성해 타일링 경고 제거
        var texMid = new Texture2D(1, 1, TextureFormat.RGBA32, false); texMid.SetPixel(0, 0, Color.white); texMid.Apply();
        var texLeft = new Texture2D(1, 1, TextureFormat.RGBA32, false); texLeft.SetPixel(0, 0, Color.white); texLeft.Apply();

        pxMid = Sprite.Create(texMid, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        pxLeft = Sprite.Create(texLeft, new Rect(0, 0, 1, 1), new Vector2(0.0f, 0.5f), 100f, 0, SpriteMeshType.FullRect);

        root = new GameObject("HB_Root").transform;
        root.SetParent(transform, false);

        // 사이즈 제어 위해 Sliced 사용 (FullRect로 경고 해결됨)
        srBG = NewSR("HP_BG", bgColor, sortingOrder, pxMid, SpriteDrawMode.Sliced);
        srFill = NewSR("HP_Fill", fillHigh, sortingOrder + 1, pxLeft, SpriteDrawMode.Sliced);

        LayoutStatic();
        UpdateBarImmediate();
    }

    SpriteRenderer NewSR(string n, Color c, int order, Sprite sprite, SpriteDrawMode mode)
    {
        var go = new GameObject(n);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = c;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = order;
        sr.drawMode = mode;
        return sr;
    }

    void LateUpdate()
    {
        Vector3 p = transform.lossyScale;
        const float eps = 1e-6f;
        root.localScale = new Vector3(1f / Mathf.Max(Mathf.Abs(p.x), eps),
                                      1f / Mathf.Max(Mathf.Abs(p.y), eps), 1f);
        root.rotation = Quaternion.identity;

        root.position = new Vector3(transform.position.x, GetTopY() + yOffset, 0f);

        UpdateBarImmediate();
    }

    float GetTopY()
    {
        if (enemyCol != null) return enemyCol.bounds.max.y;
        if (enemySR != null) return enemySR.bounds.max.y;
        return transform.position.y;
    }

    void LayoutStatic()
    {
        srBG.size = new Vector2(width, height);
        srBG.transform.localPosition = Vector3.zero;

        srFill.transform.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
        srFill.size = new Vector2(width, height);
    }

    void UpdateBarImmediate()
    {
        float maxHP = 100f, curHP = 100f;
        if (enemy != null && enemy.stats != null)
        {
            maxHP = Mathf.Max(1, enemy.stats.maxHP);
            curHP = Mathf.Clamp(enemy.stats.currentHP, 0, enemy.stats.maxHP);
        }
        float ratio = Mathf.Clamp01(curHP / maxHP);

        srFill.color = (ratio <= lowHpThreshold) ? fillLow : fillHigh;
        srFill.size = new Vector2(width * ratio, height);

        if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
    }
}

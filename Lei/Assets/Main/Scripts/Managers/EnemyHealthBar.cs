using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Layout")]
    public float width = 4f;
    public float height = 0.25f;
    public float yOffset = 1.8f;

    [Header("Colors")]
    public Color bgColor = new Color(0f, 0f, 0f, 0.65f);
    public Color fillHigh = new Color(0.20f, 0.85f, 0.20f, 0.95f);
    public Color fillLow = new Color(0.95f, 0.20f, 0.20f, 0.95f);
    public float lowHpThreshold = 0.30f;

    [Header("Sorting")]
    public string sortingLayerName = "Enemy";
    public int sortingOrder = 110;

    Transform root;
    SpriteRenderer srBG;
    SpriteRenderer srFill;
    Sprite pxMid, pxLeft;

    EnemyBase enemy;
    EnemyHealth enemyHealth;
    EnemyStats enemyStats;
    SpriteRenderer enemySR;
    Collider2D enemyCol;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>() ?? GetComponentInParent<EnemyBase>() ?? GetComponentInChildren<EnemyBase>();
        enemyHealth = GetComponent<EnemyHealth>() ?? GetComponentInParent<EnemyHealth>() ?? GetComponentInChildren<EnemyHealth>();
        enemyStats = GetComponent<EnemyStats>() ?? GetComponentInParent<EnemyStats>() ?? GetComponentInChildren<EnemyStats>();
        enemySR = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        enemyCol = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();

        if (enemySR != null)
        {
            sortingLayerName = enemySR.sortingLayerName;
            sortingOrder = enemySR.sortingOrder + 10;
        }

        var texMid = new Texture2D(1, 1, TextureFormat.RGBA32, false); texMid.SetPixel(0, 0, Color.white); texMid.Apply();
        var texLeft = new Texture2D(1, 1, TextureFormat.RGBA32, false); texLeft.SetPixel(0, 0, Color.white); texLeft.Apply();

        pxMid = Sprite.Create(texMid, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        pxLeft = Sprite.Create(texLeft, new Rect(0, 0, 1, 1), new Vector2(0.0f, 0.5f), 100f, 0, SpriteMeshType.FullRect);

        root = new GameObject("HP_Root").transform;
        root.SetParent(transform, false);

        srBG = NewSR("HP_BG", bgColor, sortingOrder, pxMid, SpriteDrawMode.Sliced);
        srFill = NewSR("HP_Fill", fillHigh, sortingOrder + 1, pxLeft, SpriteDrawMode.Sliced);

        srBG.transform.SetParent(root, false);
        srFill.transform.SetParent(root, false);

        LayoutStatic();
        UpdateBarImmediate();

        // 부모 스케일 보정
        root.localScale = Vector3.one * (1f / Mathf.Max(0.0001f, transform.lossyScale.x));
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

    void LayoutStatic()
    {
        srBG.size = new Vector2(width, height);
        srBG.transform.localPosition = Vector3.zero;

        srFill.transform.localPosition = new Vector3(-width * 0.5f, 0f, 0f);
        srFill.size = new Vector2(width, height);
    }

    void LateUpdate()
    {
        Vector3 p = transform.lossyScale;
        const float eps = 1e-6f;
        root.localScale = new Vector3(
            1f / Mathf.Max(Mathf.Abs(p.x), eps),
            1f / Mathf.Max(Mathf.Abs(p.y), eps),
            1f
        );
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

    void UpdateBarImmediate()
    {
        float maxHPf = 1f, curHPf = 1f;

        if (enemyHealth != null)
        {
            maxHPf = Mathf.Max(1f, enemyHealth.MaxHP);
            curHPf = Mathf.Clamp(enemyHealth.CurrentHP, 0f, maxHPf);
        }
        else if (enemyStats != null)
        {
            maxHPf = Mathf.Max(1f, enemyStats.maxHP);
            curHPf = Mathf.Clamp(enemyStats.currentHP, 0f, maxHPf);
        }

        float ratio = Mathf.Clamp01(maxHPf <= 0f ? 0f : (curHPf / maxHPf));

        srFill.color = Color.Lerp(fillLow, fillHigh, ratio);
        if (ratio <= lowHpThreshold)
        {
            var c = srFill.color; c.a = Mathf.Clamp01(c.a + 0.05f);
            srFill.color = c;
        }

        srFill.size = new Vector2(width * ratio, height);
    }
}

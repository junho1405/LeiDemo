using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Refs")]
    [SerializeField] private BoxCollider2D boxCol;

    private void Awake()
    {
        if (boxCol == null) boxCol = GetComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
        boxCol.enabled = false;
        Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
    }

    // === 애니메이션 이벤트에서 호출 ===
    public void EnableHitbox()
    {
        if (boxCol == null) return;
        boxCol.enabled = true;
        Debug.Log("<color=green>플레이어 공격 판정 ON</color>");
    }

    // === 애니메이션 이벤트에서 호출 ===
    public void DisableHitbox()
    {
        if (boxCol == null) return;
        boxCol.enabled = false;
        Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!boxCol.enabled) return;

        // 적 루트는 Tag="Enemy"
        if (!other.CompareTag("Enemy")) return;

        // 비트리거 콜라이더 + Rigidbody2D가 Enemy 쪽에 있어야 트리거가 동작함
        var rb = other.attachedRigidbody;
        if (rb == null)
        {
            Debug.Log("<color=orange>Enemy Rigidbody2D 없음 → 충돌 안 잡힐 수 있음</color>");
        }

        var hp = other.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            Debug.Log($"<color=yellow>적 피격! → {other.name} / {damage}</color>");
        }
        else
        {
            Debug.Log("<color=orange>EnemyHealth 없음 → 데미지 미적용</color>");
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("공격력 설정")]
    public int damage = 10;

    private bool canHit = false;      // 공격 중일 때만 true
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 공격 중이 아니면 무시
        if (!canHit) return;

        // 플레이어 충돌 감지
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"<color=red>[EnemyHitbox]</color> 플레이어 피격! 데미지: {damage}");
            }
        }
        Debug.Log($"[Hitbox Debug] 충돌 감지 대상: {other.name}"); //이 줄 추가
        if (!canHit) return;
    }

    // --- 애니메이션 이벤트에서 호출됨 ---
    public void EnableHitbox()
    {
        canHit = true;
        Debug.Log("<color=green>고블린 공격 판정 ON</color>");
    }

    public void DisableHitbox()
    {
        canHit = false;
        Debug.Log("<color=gray>고블린 공격 판정 OFF</color>");
    }

#if UNITY_EDITOR
    // --- Scene 뷰에서 히트박스 시각화 ---
    private void OnDrawGizmos()
    {
        Gizmos.color = canHit ? Color.red : Color.gray;
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider2D box)
                Gizmos.DrawWireCube(box.offset, box.size);
        }
    }
#endif
}

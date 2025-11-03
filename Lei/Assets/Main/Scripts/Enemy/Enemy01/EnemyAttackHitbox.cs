using UnityEngine;
using static Balance;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("공격력 설정")]
    public int damage = 10;

    private bool canHit = false;
    private Collider2D hitboxCollider;
    private EnemyBase owner; // ★ 루트 적 참조 (티어/상태용)

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false; // 기본 비활성
        owner = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit) return;

        if (other.CompareTag("Player"))
        {
            // 패링 체크
            HeroKnight player = other.GetComponent<HeroKnight>() ?? other.GetComponentInParent<HeroKnight>();
            if (player != null && player.IsParryActive)
            {
                player.OnParrySuccess(transform.root.gameObject); // 적 루트 전달
                return;
            }

            // 플레이어 피해 (티어별 “주는 피해” 배율 적용)
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                float outMul = owner ? Balance.OutgoingDamageMultiplier(owner.tier) : 1f;
                int final = Mathf.RoundToInt(damage * outMul);
                playerHealth.TakeDamage(final);
                Debug.Log($"플레이어 피격! {final} 데미지 (tier {(owner ? owner.tier : EnemyTier.Normal)})");
            }
        }
    }

    // --- Animation Event용 ---
    public void EnableHitbox()
    {
        if (canHit) return; // 중복 가드
        canHit = true;

        // 재진입 보장
        hitboxCollider.enabled = false;
        hitboxCollider.enabled = true;
        Debug.Log("<color=green>고블린 공격 판정 ON</color>");
    }

    public void DisableHitbox()
    {
        if (!canHit) return; // 중복 가드
        canHit = false;

        hitboxCollider.enabled = false;
        Debug.Log("<color=gray>고블린 공격 판정 OFF</color>");
    }
}

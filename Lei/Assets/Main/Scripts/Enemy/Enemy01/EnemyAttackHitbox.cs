using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("공격력 설정")]
    public int damage = 10;

    private bool canHit = false;      // 애니메이션 중 공격 타이밍 제어
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit) return;

        // 플레이어 태그 또는 PlayerHealth 컴포넌트 확인
        if (other.CompareTag("Player") || other.TryGetComponent(out PlayerHealth player))
        {
            // PlayerHealth 스크립트 탐색
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"플레이어 피격! {damage} 데미지");
            }
            else
            {
                Debug.LogWarning("PlayerHealth 스크립트를 찾을 수 없습니다!");
            }
        }
    }

    // 애니메이션 이벤트용 함수
    public void EnableHitbox()
    {
        canHit = true;
        Debug.Log("고블린 공격 판정 ON");
    }

    public void DisableHitbox()
    {
        canHit = false;
        Debug.Log("고블린 공격 판정 OFF");
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("공격력 설정")]
    public int damage = 10;

    private bool canHit = false;
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit) return;

        if (other.CompareTag("Player"))
        {
            HeroKnight player = other.GetComponent<HeroKnight>();
            if (player != null && player.IsParryActive)
            {
                player.OnParrySuccess(gameObject);
                return;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"플레이어 피격! {damage} 데미지");
            }
        }
    }

    // --- Animation Event용 ---
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
}

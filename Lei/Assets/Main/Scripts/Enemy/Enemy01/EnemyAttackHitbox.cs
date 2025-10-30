using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("���ݷ� ����")]
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
                Debug.Log($"�÷��̾� �ǰ�! {damage} ������");
            }
        }
    }

    // --- Animation Event�� ---
    public void EnableHitbox()
    {
        canHit = true;
        Debug.Log("<color=green>��� ���� ���� ON</color>");
    }

    public void DisableHitbox()
    {
        canHit = false;
        Debug.Log("<color=gray>��� ���� ���� OFF</color>");
    }
}

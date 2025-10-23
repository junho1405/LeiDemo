using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("���ݷ� ����")]
    public int damage = 10;

    private bool canHit = false;      // ���� ���� ���� true
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���� ���� �ƴϸ� ����
        if (!canHit) return;

        // �÷��̾� �浹 ����
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"<color=red>[EnemyHitbox]</color> �÷��̾� �ǰ�! ������: {damage}");
            }
        }
        Debug.Log($"[Hitbox Debug] �浹 ���� ���: {other.name}"); //�� �� �߰�
        if (!canHit) return;
    }

    // --- �ִϸ��̼� �̺�Ʈ���� ȣ��� ---
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

#if UNITY_EDITOR
    // --- Scene �信�� ��Ʈ�ڽ� �ð�ȭ ---
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

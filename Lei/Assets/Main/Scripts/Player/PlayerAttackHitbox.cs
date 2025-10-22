using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("���� ������")]
    public int Damage = 10;

    private BoxCollider2D boxCollider;
    private bool isActive = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D�� �����ϴ�! AttackHitbox�� �߰����ּ���.");
        }

        // �⺻�� ��Ȱ��ȭ (��� ����)
        boxCollider.enabled = false;
    }

    // --- AnimationEvent���� ȣ��� ---
    public void EnableHitbox()
    {
        if (boxCollider == null) return;

        isActive = true;
        boxCollider.enabled = true;
        Debug.Log("<color=green>�÷��̾� ���� ���� ON</color>");
    }

    public void DisableHitbox()
    {
        if (boxCollider == null) return;

        isActive = false;
        boxCollider.enabled = false;
        Debug.Log("<color=red>�÷��̾� ���� ���� OFF</color>");
    }

    // --- �浹 ���� ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;  // Ȱ��ȭ ���°� �ƴ� �� ����

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"<color=yellow>�� �ǰ�! �� {other.name}</color>");
            EnemyBase enemy = other.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(Damage);
            }
            else
            {
                Debug.LogWarning("EnemyBase ��ũ��Ʈ�� ã�� �� �����ϴ�!");
            }
        }
    }

#if UNITY_EDITOR
    // --- Scene �信�� ���� ���� �ð�ȭ (����׿�) ---
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.red : Color.gray;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
#endif
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("���ݷ� ����")]
    public int damage = 10;

    private bool canHit = false;      // �ִϸ��̼� �� ���� Ÿ�̹� ����
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit) return;

        // �÷��̾� �±� �Ǵ� PlayerHealth ������Ʈ Ȯ��
        if (other.CompareTag("Player") || other.TryGetComponent(out PlayerHealth player))
        {
            // PlayerHealth ��ũ��Ʈ Ž��
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"�÷��̾� �ǰ�! {damage} ������");
            }
            else
            {
                Debug.LogWarning("PlayerHealth ��ũ��Ʈ�� ã�� �� �����ϴ�!");
            }
        }
    }

    // �ִϸ��̼� �̺�Ʈ�� �Լ�
    public void EnableHitbox()
    {
        canHit = true;
        Debug.Log("��� ���� ���� ON");
    }

    public void DisableHitbox()
    {
        canHit = false;
        Debug.Log("��� ���� ���� OFF");
    }
}

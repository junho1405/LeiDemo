using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("공격 데미지")]
    public int Damage = 10;

    private BoxCollider2D boxCollider;
    private bool isActive = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (!boxCollider) Debug.LogError("BoxCollider2D가 없습니다! AttackHitbox에 추가해주세요.");
        if (boxCollider) boxCollider.enabled = false; // 기본 비활성
    }

    public void EnableHitbox()
    {
        if (!boxCollider) return;
        if (isActive) return;
        isActive = true;

        boxCollider.enabled = false;
        boxCollider.enabled = true;
        Debug.Log("<color=green>플레이어 공격 판정 ON</color>");
    }

    public void DisableHitbox()
    {
        if (!boxCollider) return;
        if (!isActive) return;
        isActive = false;

        boxCollider.enabled = false;
        Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>() ?? other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                Debug.Log($"<color=yellow>적 피격! → {enemy.gameObject.name}</color>");
                enemy.TakeDamage(Damage); // ★ 배율 적용은 EnemyBase.TakeDamage 내에서 수행
            }
            else
            {
                Debug.LogWarning("EnemyBase 스크립트를 찾을 수 없습니다!");
            }
        }
    }

#if UNITY_EDITOR
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

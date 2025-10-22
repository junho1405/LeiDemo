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
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider2D가 없습니다! AttackHitbox에 추가해주세요.");
        }

        // 기본은 비활성화 (대기 상태)
        boxCollider.enabled = false;
    }

    // --- AnimationEvent에서 호출됨 ---
    public void EnableHitbox()
    {
        if (boxCollider == null) return;

        isActive = true;
        boxCollider.enabled = true;
        Debug.Log("<color=green>플레이어 공격 판정 ON</color>");
    }

    public void DisableHitbox()
    {
        if (boxCollider == null) return;

        isActive = false;
        boxCollider.enabled = false;
        Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
    }

    // --- 충돌 감지 ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;  // 활성화 상태가 아닐 때 무시

        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"<color=yellow>적 피격! → {other.name}</color>");
            EnemyBase enemy = other.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(Damage);
            }
            else
            {
                Debug.LogWarning("EnemyBase 스크립트를 찾을 수 없습니다!");
            }
        }
    }

#if UNITY_EDITOR
    // --- Scene 뷰에서 공격 범위 시각화 (디버그용) ---
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

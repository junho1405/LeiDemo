using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float applyCooldown = 0.25f; // 동일 대상 연타 방지

    [Header("Refs")]
    [SerializeField] private BoxCollider2D boxCol;

    private float _nextApplyTime;
    private EnemyBase _owner;

    private void Awake()
    {
        if (boxCol == null) boxCol = GetComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
        boxCol.enabled = false;

        _owner = GetComponentInParent<EnemyBase>();
    }

    // 애니메이션 이벤트에서 호출
    public void EnableHitbox()
    {
        if (boxCol == null) return;
        boxCol.enabled = true;
        Debug.Log("<color=gray>적 공격 판정 ON</color>");
    }

    // 애니메이션 이벤트에서 호출
    public void DisableHitbox()
    {
        if (boxCol == null) return;
        boxCol.enabled = false;
        Debug.Log("<color=gray>적 공격 판정 OFF</color>");
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleHit(other);
    private void OnTriggerStay2D(Collider2D other) => HandleHit(other);

    private void HandleHit(Collider2D other)
    {
        if (!boxCol.enabled) return;
        if (Time.time < _nextApplyTime) return;
        if (!other.CompareTag("Player")) return;

        // 1) 패링 창 체크
        var parry = other.GetComponent<PlayerParryBridge>();
        if (parry != null && parry.IsWindowActive)
        {
            // 공격 취소 + 히트박스 OFF + 재적용 쿨다운 + 시퀀스 시작
            _owner?.EndAttack();
            DisableHitbox();
            _nextApplyTime = Time.time + applyCooldown;

            if (ParrySequenceSystem.Instance != null && _owner != null)
            {
                ParrySequenceSystem.Instance.Begin(_owner.transform);
                Debug.Log("<color=#00E5FF>[Parry] SUCCESS → 시퀀스 시작</color>");
            }
            else
            {
                Debug.Log("<color=#00E5FF>[Parry] SUCCESS (시퀀스 없음)</color>");
            }
            return;
        }

        // 2) 일반 피격
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            _nextApplyTime = Time.time + applyCooldown;
            Debug.Log($"<color=yellow>플레이어 피격 {damage}</color>");
        }
        else
        {
            Debug.Log("<color=orange>PlayerHealth 없음 → 데미지 미적용</color>");
        }
    }
}

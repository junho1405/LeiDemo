using UnityEngine;

[DisallowMultipleComponent]
public class EnemyNormal : EnemyBase
{
    [Header("Goblin Tuning")]
    [Tooltip("0이면 EnemyBase.moveSpeed 사용")]
    public float overrideMoveSpeed = 0f;

    protected override void Awake()
    {
        base.Awake();
        tier = EnemyTier.Normal;
    }

    protected override void Start()
    {
        base.Start(); // Animator 파라미터 캐시 등 부모 로직 사용
    }

    // === 행동 ===
    public override void Idle()
    {
        // 부모의 SetRun(false) 포함
        base.Idle();
        // 필요 시 전용 Idle 클립을 강제 재생하려면 아래 주석 해제
        // if (animator) animator.Play("Goblin_Idle", 0, 0f);
    }

    public override void MoveTowardsPlayer()
    {
        if (target == null) return;
        if (health != null && health.IsDead) return;

        if (overrideMoveSpeed > 0f) moveSpeed = overrideMoveSpeed;

        // 이동/회전/달리기 Bool 처리는 부모 표준 로직 사용
        base.MoveTowardsPlayer();

        // 필요 시 전용 Run 클립을 강제 재생하려면 아래 주석 해제
        // if (animator) animator.Play("Goblin_Run", 0, 0f);
    }

    public override void Attack()
    {
        // 공격 트리거/쿨다운/히트박스 이벤트 흐름은 부모 표준 로직 사용
        base.Attack();

        // 필요 시 전용 Attack 클립을 강제 재생하려면 아래 주석 해제
        // if (animator) animator.Play("Goblin_Attack1", 0, 0f);
        //
        // ※ Attack 애니메이션 클립에는 Animation Event가 반드시 있어야 함:
        //    EnableHitbox() / DisableHitbox() / EndAttack()
    }

    // Die()는 오버라이드하지 않음 → 부모의 즉시 비활성화(사망) 로직 사용

#if UNITY_EDITOR
    // 디버그용 범위 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}

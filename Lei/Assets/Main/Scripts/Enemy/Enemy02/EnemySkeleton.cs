using UnityEngine;

[DisallowMultipleComponent]
public class EnemySkeleton : EnemyBase
{
    [Header("Skeleton Tuning")]
    public float walkSpeed = 1.5f;

    [Header("Detection / Attack")]
    public float verticalTolerance = 2.0f; // 플레이어와의 높이 차이 허용 범위

    protected override void Awake()
    {
        base.Awake();
        tier = EnemyTier.Boss;                 // 보스 취급
        attackTriggerName = "Skeleton_Attack1"; // 스켈레톤 전용 트리거명
    }

    public override void Idle()
    {
        if (isAttacking || isDead) return;
        if (anim) anim.Play("Skeleton_Idle");
        SetRun(false);
    }

    public override void MoveTowardsPlayer()
    {
        if (!player || isDead || isAttacking) return;

        // 수평/수직 거리 분리 계산
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);

        // 수직 높이 차이가 너무 크면 공격하지 않음
        if (distY > verticalTolerance)
        {
            Idle();
            return;
        }

        // 공격 사거리 안 → 이동 멈추고 공격
        if (distX <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            SetRun(false);
            Attack();
            return;
        }

        // 공격 사거리 밖 → 이동
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * walkSpeed, rb.linearVelocity.y);
        if (anim) anim.Play("Skeleton_Walk");
        FaceToTargetIfNeeded();
    }

    public override void Attack()
    {
        if (isDead || isAttacking) return;
        StartCoroutine(AttackRoutine()); // EnemyBase 공격 루틴 사용 (쿨타임/상태 일괄 관리)
    }

    protected override void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            Idle();
            return;
        }

        // 수평/수직 거리 계산
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);

        // 탐지 범위 밖
        if (distX > detectionRange)
        {
            Idle();
            return;
        }

        // 공격 중이면 방향만 유지
        if (isAttacking)
        {
            FaceToTargetIfNeeded();
            return;
        }

        // 수직 높이 차이가 크면 대기
        if (distY > verticalTolerance)
        {
            Idle();
            return;
        }

        // 이동/공격 처리
        MoveTowardsPlayer();
    }

    // ★ 접근 지정자 수정: base가 public virtual Die()이므로 public override로 맞춤
    public override void Die()
    {
        base.Die();
        if (anim) anim.Play("Skeleton_Death", 0, 0f);
        rb.simulated = false;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }
}

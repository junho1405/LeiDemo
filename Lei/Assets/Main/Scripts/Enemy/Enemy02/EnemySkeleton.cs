using UnityEngine;

[DisallowMultipleComponent]
public class EnemySkeleton : EnemyBase
{
    [Header("Skeleton Move Speed")]
    public float walkSpeed = 1.5f;

    protected override void Awake()
    {
        base.Awake();
        tier = EnemyTier.Boss; // 보스로 취급
    }

    public override void Idle()
    {
        SetRun(false);
        if (animator != null) animator.Play("Skeleton_Idle");
    }

    public override void MoveTowardsPlayer()
    {
        if (target == null) return;
        if (health != null && health.IsDead) return;

        float dir = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * walkSpeed, rb.linearVelocity.y);

        if (animator != null) animator.Play("Skeleton_Walk");

        if (Vector2.Distance(target.position, transform.position) <= attackRange)
            Attack();
    }

    public override void Attack()
    {
        if (health != null && health.IsDead) return;
        if (isAttacking) return;

        isAttacking = true;
        if (animator != null) animator.Play("Skeleton_Attack1", 0, 0f);
        // AnimationEvent: EnableHitbox / DisableHitbox / EndAttack
    }

    protected override void Die()
    {
        base.Die();
        if (animator != null) animator.Play("Skeleton_Death", 0, 0f);
        rb.simulated = false;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}

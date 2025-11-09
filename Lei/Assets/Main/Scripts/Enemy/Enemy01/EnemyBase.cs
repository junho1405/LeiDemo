using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Detect / Move")]
    public float detectRange = 8f;
    public float attackRange = 1.8f;
    public float moveSpeed = 2.5f;
    public bool faceToTarget = true;

    [Header("Animator Param Names")]
    public string attackTriggerName = "Attack1";
    public string runBoolName = "isRunning";

    [Header("Attack Timing")]
    public float attackCooldown = 1.2f;
    public float attackAnimMaxTime = 1.0f;

    [Header("Stats / References")]
    public EnemyTier tier = EnemyTier.Normal;
    public EnemyStats stats;            // HP / Stagger / Fever 플래그
    public EnemyHealth health;          // HP 감소
    public EnemyAttackHitbox attackHitbox;
    public Transform target;

    [Header("Fever Time Settings")]
    public float feverDuration = 10f;          // 지속시간(초)
    public float feverDamageMultiplier = 1.5f; // 피버 중 피해 배율

    protected Animator animator;
    protected Rigidbody2D rb;
    protected bool isAttacking = false;
    protected float nextAttackTime = 0f;

    bool _hasRunBool;
    bool _hasAttackTrigger;

    public float CurrentStagger => (stats != null) ? stats.currentStagger : 0f;
    public float MaxStagger => (stats != null) ? stats.maxStagger : 0f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (stats == null) stats = GetComponent<EnemyStats>();
        if (attackHitbox == null) attackHitbox = GetComponentInChildren<EnemyAttackHitbox>(true);
    }

    protected virtual void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        RefreshAnimatorParamCache();
    }

    protected virtual void Update()
    {
        // ✅ HP가 0이 되어 EnemyHealth.IsDead 가 true면 즉시 제거
        if (health != null && health.IsDead)
        {
            Die();
            return;
        }

        if (target == null) return;

        // ✅ 피버 중에는 이동/공격 정지
        if (stats != null && stats.isInFever)
        {
            rb.linearVelocity = Vector2.zero;
            SetRun(false);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectRange)
        {
            Idle();
            return;
        }

        if (isAttacking)
        {
            FaceTarget();
            SetRun(false);
            return;
        }

        if (dist > attackRange)
        {
            MoveTowardsPlayer();
            return;
        }

        if (Time.time >= nextAttackTime)
            StartCoroutine(AttackRoutine());
        else
            SetRun(false);
    }

    // --- 행동 ---
    public virtual void Idle()
    {
        SetRun(false);
    }

    public virtual void MoveTowardsPlayer()
    {
        if (target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);

        FaceTarget();
        SetRun(Mathf.Abs(rb.linearVelocity.x) > 0.05f);
    }

    public virtual void Attack()
    {
        if (_hasAttackTrigger)
        {
            animator.ResetTrigger(attackTriggerName);
            animator.SetTrigger(attackTriggerName);
        }
    }

    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        SetRun(false);

        Attack();

        float t = 0f;
        while (t < attackAnimMaxTime && isAttacking)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (isAttacking)
            EndAttack();
    }

    public virtual void EndAttack()
    {
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    public virtual void EnableHitbox()
    {
        if (attackHitbox) attackHitbox.EnableHitbox();
    }

    public virtual void DisableHitbox()
    {
        if (attackHitbox) attackHitbox.DisableHitbox();
    }

    // --- 데미지 ---
    public virtual void TakeDamage(int dmg)
    {
        if (stats != null && stats.isInFever)
            dmg = Mathf.RoundToInt(dmg * feverDamageMultiplier);

        if (health != null) health.TakeDamage(dmg);
        if (stats != null) stats.currentHP = Mathf.Max(0, stats.currentHP - dmg);

        if (stats != null && stats.currentHP <= 0)
            Die();
    }

    public virtual void TakeParryDamage(int dmg, float staggerReduce)
    {
        if (stats != null && stats.isInFever)
            dmg = Mathf.RoundToInt(dmg * feverDamageMultiplier);

        if (health != null) health.TakeDamage(dmg);

        if (stats != null)
        {
            stats.currentStagger = Mathf.Max(0, stats.currentStagger - Mathf.Abs(staggerReduce));
            if (stats.currentStagger <= 0f && !stats.isInFever)
                TriggerFeverMode();
        }
    }

    public virtual void ReduceStagger(float v)
    {
        if (stats == null) return;
        stats.currentStagger = Mathf.Max(0, stats.currentStagger - Mathf.Abs(v));

        if (stats.currentStagger <= 0f && !stats.isInFever)
            TriggerFeverMode();
    }

    // ✅ 피버타임 전용 데미지 (FeverSequenceSystem에서 호출)
    public virtual void TakeFeverDamage(int dmg)
    {
        if (health != null) health.TakeDamage(dmg);
        if (stats != null) stats.currentHP = Mathf.Max(0, stats.currentHP - dmg);

        if (stats != null && stats.currentHP <= 0)
            Die();
    }

    // --- Fever ---
    public void TriggerFeverMode()
    {
        if (stats == null || stats.isInFever) return;

        // 발동 즉시 경직도 풀회복
        stats.currentStagger = stats.maxStagger;
        stats.isInFever = true;

        // 이동/공격 잠금
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;

        FeverSequenceSystem.Instance?.BeginFever(this, feverDuration);
    }

    public void ExitFeverTime()
    {
        if (stats != null) stats.isInFever = false;
    }

    // --- 기타 ---
    protected virtual void Die()
    {
        // 피버 중이었다면 강제 종료
        FeverSequenceSystem.Instance?.ForceStopFeverBy(this);

        if (health != null) health.Die();

        // 즉시 제거
        gameObject.SetActive(false);
    }

    protected void SetRun(bool running)
    {
        if (_hasRunBool) animator.SetBool(runBoolName, running);
        if (!running) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected void FaceTarget()
    {
        if (!faceToTarget || target == null) return;

        float dx = target.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.001f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dx);
            transform.localScale = s;
        }
    }

    protected void RefreshAnimatorParamCache()
    {
        _hasRunBool = HasAnimatorParam(animator, runBoolName, AnimatorControllerParameterType.Bool);
        _hasAttackTrigger = HasAnimatorParam(animator, attackTriggerName, AnimatorControllerParameterType.Trigger);
    }

    static bool HasAnimatorParam(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (anim == null || string.IsNullOrEmpty(name)) return false;
        foreach (var p in anim.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    // PlayerSpawner에서 호출
    public void SetPlayer(Transform t) => target = t;
}

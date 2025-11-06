using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Detect / Move")]
    [SerializeField] protected float detectRange = 8f;
    [SerializeField] protected float attackRange = 2.8f;
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] protected bool faceToTarget = true;

    [Header("Animator Param Names")]
    [SerializeField] protected string attackTriggerName = "Attack1";
    [SerializeField] protected string runBoolName = "isRunning";

    [Header("Attack Timing")]
    [SerializeField] protected float attackCooldown = 1.2f;
    [SerializeField] protected float attackAnimMaxTime = 1.0f;

    [Header("Stats / Tier")]
    public EnemyTier tier = EnemyTier.Normal;
    public EnemyStats stats;

    [Header("Refs")]
    [SerializeField] protected EnemyAttackHitbox attackHitbox;
    [SerializeField] protected Transform target;
    [SerializeField] protected EnemyHealth health;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected bool isAttacking = false;
    protected float nextAttackTime = 0f;

    // 하위호환 별칭/프로퍼티
    protected Animator anim => animator;
    protected Transform player { get => target; set => target = value; }
    public float detectionRange { get => detectRange; set => detectRange = value; }
    public bool isDead => (health != null && health.IsDead);

    // Stagger 프록시
    public float CurrentStagger => stats != null ? stats.currentStagger : 0f;
    public float MaxStagger => stats != null ? stats.maxStagger : 0f;
    public bool HasStagger => stats != null && stats.currentStagger > 0f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (attackHitbox == null) attackHitbox = GetComponentInChildren<EnemyAttackHitbox>(true);
        if (health == null) health = GetComponent<EnemyHealth>();
        if (stats == null) stats = GetComponent<EnemyStats>();
    }

    protected virtual void Start()
    {
        if (target == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) target = playerGo.transform;
        }
        RefreshAnimatorParamCache();
    }

    protected virtual void Update()
    {
        if (target == null || isDead) return;

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectRange)
        {
            SetRun(false);
            return;
        }

        if (isAttacking)
        {
            FaceToTargetIfNeeded();
            SetRun(false);
            return;
        }

        if (dist > attackRange)
        {
            MoveToward(target.position);
        }
        else if (Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            SetRun(false);
        }
    }

    // ---------- 이동/방향 ----------
    protected void MoveToward(Vector3 goal)
    {
        Vector2 dir = (goal - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        FaceToTargetIfNeeded();
        SetRun(Mathf.Abs(rb.linearVelocity.x) > 0.05f);
    }

    protected void SetRun(bool running)
    {
        if (!string.IsNullOrEmpty(runBoolName))
            animator.SetBool(runBoolName, running);
        if (!running)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    protected void FaceToTargetIfNeeded()
    {
        if (!faceToTarget || target == null) return;
        float dx = target.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.0001f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * Mathf.Sign(dx);
            transform.localScale = s;
        }
    }

    // ---------- 공격 루틴 ----------
    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SetRun(false);

        animator.ResetTrigger(attackTriggerName);
        animator.SetTrigger(attackTriggerName);

        float t = 0f;
        while (t < attackAnimMaxTime && isAttacking)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (isAttacking) EndAttack();
    }

    public virtual void EnableHitbox() => attackHitbox?.EnableHitbox();
    public virtual void DisableHitbox() => attackHitbox?.DisableHitbox();

    public virtual void EndAttack()
    {
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    // ---------- 데미지/경직 ----------
    public virtual void TakeDamage(int dmg)
    {
        if (health != null)
        {
            health.TakeDamage(dmg);
            Debug.Log($"<color=yellow>[Enemy] 피해 {dmg}</color>");
        }
    }

    // 패링 성공용 (트루 대미지 + 패링 전용 경직 감소)
    public virtual void TakeTrueDamage(int dmg, float staggerBonusMultiplier = 1.5f)
    {
        if (health != null)
            health.TakeDamage(dmg);
        else if (stats != null)
            stats.currentHP = Mathf.Max(0, stats.currentHP - dmg);

        if (stats != null)
        {
            float staggerDmg = Mathf.Max(1f, dmg * Mathf.Abs(staggerBonusMultiplier));
            stats.currentStagger = Mathf.Max(0f, stats.currentStagger - staggerDmg);
            Debug.Log($"<color=#FFAA00>[Parry] 트루대미지 {dmg}, 경직도 -{staggerDmg}</color>");
        }
    }

    // 기존 호환(일반 공격에서 호출 금지 권장)
    public virtual void ReduceStagger(float v)
    {
        if (stats == null) return;
        stats.currentStagger = Mathf.Max(0f, stats.currentStagger - Mathf.Abs(v));
    }

    public virtual void ReduceStaggerFromParry(float v)
    {
        if (stats == null) return;
        stats.currentStagger = Mathf.Max(0f, stats.currentStagger - Mathf.Abs(v));
        Debug.Log($"<color=orange>[Stagger] 패링 경직도 {v} 감소</color>");
    }

    // ---------- 기본 상태 ----------
    public virtual void Attack() { }
    public virtual void Idle() { }
    public virtual void MoveTowardsPlayer() { }

    public virtual void Die()
    {
        if (health != null)
        {
            health.Die();
        }
        else
        {
            Debug.Log($"<color=red>[EnemyBase] {name} 사망 → 오브젝트 비활성화</color>");
            gameObject.SetActive(false);
        }
    }

    // ---------- 유틸 ----------
    public void SetPlayer(Transform t) => target = t;

    protected void RefreshAnimatorParamCache() { /* 파라미터 캐시 생략(호환) */ }
}

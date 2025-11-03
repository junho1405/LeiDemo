using UnityEngine;
using static Balance;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Tier")]
    public EnemyTier tier = EnemyTier.Normal; // ★ 일반/엘리트/보스 구분

    public EnemyStats stats;
    protected Animator anim;
    protected Transform player;
    protected Rigidbody2D rb;
    protected Collider2D col;

    [Header("AI Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2.5f;
    protected bool isDead = false;
    protected bool isAttacking = false;

    public int CurrentHP => stats != null ? stats.currentHP : 0;
    public int MaxHP => stats != null ? stats.maxHP : 0;

    public virtual bool HasStagger => false;
    public virtual float CurrentStagger => 0f;
    public virtual float MaxStagger => 0f;

    [Header("Hitbox (AnimationEvent 래퍼용)")]
    [SerializeField] protected EnemyAttackHitbox attackHitbox;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        stats = new EnemyStats();
        stats.Init();

        if (attackHitbox == null)
            attackHitbox = GetComponentInChildren<EnemyAttackHitbox>(true);

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.isTrigger = true;

        InvokeRepeating(nameof(FindPlayer), 0f, 1f);
    }

    void FindPlayer()
    {
        var hk = Object.FindFirstObjectByType<HeroKnight>();
        if (hk != null)
        {
            player = hk.transform;
            CancelInvoke(nameof(FindPlayer));
            Debug.Log($"{gameObject.name} found player: {player.name}");
        }
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        Vector2 enemyPos = new Vector2(transform.position.x, 0);
        Vector2 playerPos = new Vector2(player.position.x, 0);
        float distance = Vector2.Distance(enemyPos, playerPos);

        if (isAttacking) { rb.linearVelocity = Vector2.zero; return; }

        if (distance <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            Attack();
        }
        else if (distance <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            Idle();
        }
    }

    protected virtual void Idle()
    {
        rb.linearVelocity = Vector2.zero;
        anim.Play("Goblin_Idle");
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (isAttacking) return;

        anim.Play("Goblin_Run");

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * stats.moveSpeed, 0);

        if (dirX > 0) transform.localScale = new Vector3(5, 5, 1);
        else transform.localScale = new Vector3(-5, 5, 1);
    }

    protected virtual void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        int pattern = Random.Range(0, 2);
        string animName = pattern == 0 ? "Goblin_Attack1" : "Goblin_Attack2";
        Debug.Log($"{gameObject.name} 공격 발동! ({animName})");

        anim.Play(animName);

        Invoke(nameof(EndAttack), 1.0f);
    }

    protected void EndAttack() { isAttacking = false; }

    // ★ 배율 적용: 플레이어의 공격이 들어올 때
    public virtual void TakeDamage(int rawDamage)
    {
        // 플레이어가 준 피해에 티어별 “받는 피해” 배율 적용
        int adjusted = Mathf.RoundToInt(rawDamage * Balance.IncomingDamageMultiplier(tier));
        stats.TakeDamage(adjusted);
        anim.Play("Goblin_Hit");

        if (stats.IsDead()) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play("Goblin_Death");
        Destroy(gameObject, 2f);
    }

    public void SetPlayer(Transform p) { player = p; }

    // --- AnimationEvent 래퍼 ---
    public void EnableHitbox() => attackHitbox?.EnableHitbox();
    public void DisableHitbox() => attackHitbox?.DisableHitbox();
}

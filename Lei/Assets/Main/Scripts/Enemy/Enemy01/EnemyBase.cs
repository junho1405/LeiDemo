using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
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

    // 체력바용 공개 읽기 전용 값
    public int CurrentHP => stats != null ? stats.currentHP : 0;
    public int MaxHP => stats != null ? stats.maxHP : 0;

    // 경직도(스태거) — 기본: 없음(서브클래스에서 override 가능)
    public virtual bool HasStagger => false;
    public virtual float CurrentStagger => 0f;
    public virtual float MaxStagger => 0f;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        stats = new EnemyStats();
        stats.Init();

        // Rigidbody 설정
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider 설정
        col.isTrigger = true;

        InvokeRepeating(nameof(FindPlayer), 0f, 1f);
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            CancelInvoke(nameof(FindPlayer));
            Debug.Log($"{gameObject.name} found player: {player.name}");
        }
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        // y축 무시한 평면 거리 계산
        Vector2 enemyPos = new Vector2(transform.position.x, 0);
        Vector2 playerPos = new Vector2(player.position.x, 0);
        float distance = Vector2.Distance(enemyPos, playerPos);

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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

        // x축만 추적
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * stats.moveSpeed, 0);

        // 방향 전환
        if (dirX > 0)
            transform.localScale = new Vector3(5, 5, 1);
        else
            transform.localScale = new Vector3(-5, 5, 1);
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

    protected void EndAttack()
    {
        isAttacking = false;
    }

    public virtual void TakeDamage(int dmg)
    {
        stats.TakeDamage(dmg);
        anim.Play("Goblin_Hit");

        if (stats.IsDead())
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.Play("Goblin_Death");
        Destroy(gameObject, 2f);
    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }
}

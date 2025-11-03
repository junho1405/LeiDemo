using UnityEngine;
using UnityEngine.SceneManagement;
using static Balance;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;

    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    // --- 패링 관련 ---
    private bool isParryActive = false;
    private float parryTimer = 0f;

    // ★ 패링 쿨다운/락/경직
    private float parryCooldownTimer = 0f;   // 패링 재사용 불가
    private float attackParryLockTimer = 0f; // 공격/패링 입력 잠금
    private float stunnedTimer = 0f;         // 0.26s 경직
    private bool parryWindowOpened = false;  // 이번 창이 열렸는지
    private bool parrySucceeded = false;     // 이번 창에 성공했는지

    public float ParryCooldownRemaining => Mathf.Max(0f, parryCooldownTimer);
    public float AttackParryLockRemaining => Mathf.Max(0f, attackParryLockTimer);
    public float StunnedRemaining => Mathf.Max(0f, stunnedTimer);

    public bool IsParryActive => isParryActive;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2")?.GetComponent<Sensor_HeroKnight>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (m_groundSensor != null)
        {
            m_groundSensor.Disable(0.05f);
            Debug.Log($"[HeroKnight] Scene loaded: {scene.name}, 센서 초기화 완료");
        }
        else
        {
            Debug.LogWarning($"[HeroKnight] Scene loaded: {scene.name}, GroundSensor 연결 안됨");
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        m_timeSinceAttack += dt;
        if (m_rolling) m_rollCurrentTime += dt;
        if (m_rollCurrentTime > m_rollDuration) m_rolling = false;

        // === 타이머 업데이트 ===
        if (parryCooldownTimer > 0f) parryCooldownTimer -= dt;
        if (attackParryLockTimer > 0f) attackParryLockTimer -= dt;
        if (stunnedTimer > 0f) stunnedTimer -= dt;

        // 착지 감지
        if (!m_grounded && m_groundSensor != null && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", true);
        }
        if (m_grounded && (m_groundSensor == null || !m_groundSensor.State()))
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
        }

        // 이동 입력 (경직 중에는 이동 불가)
        float inputX = stunnedTimer > 0f ? 0f : Input.GetAxis("Horizontal");
        if (inputX > 0) { GetComponent<SpriteRenderer>().flipX = false; m_facingDirection = 1; }
        else if (inputX < 0) { GetComponent<SpriteRenderer>().flipX = true; m_facingDirection = -1; }

        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
        m_isWallSliding =
            (m_wallSensorR1 && m_wallSensorR2 && m_wallSensorR1.State() && m_wallSensorR2.State()) ||
            (m_wallSensorL1 && m_wallSensorL2 && m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        // ===== 입력 처리 =====
        if (stunnedTimer <= 0f)
        {
            if (Input.GetKeyDown("e") && !m_rolling) { m_animator.SetBool("noBlood", m_noBlood); m_animator.SetTrigger("Death"); }
            else if (Input.GetKeyDown("q") && !m_rolling) m_animator.SetTrigger("Hurt");

            // 공격 (락 중엔 불가)
            else if (Input.GetMouseButtonDown(0) && attackParryLockTimer <= 0f && m_timeSinceAttack > 0.25f && !m_rolling && !isParryActive)
            {
                m_currentAttack++;
                if (m_currentAttack > 3) m_currentAttack = 1;
                if (m_timeSinceAttack > 1.0f) m_currentAttack = 1;
                m_animator.SetTrigger("Attack" + m_currentAttack);
                m_timeSinceAttack = 0.0f;
            }

            // 패링 (쿨다운/락 중엔 불가)
            else if (Input.GetMouseButtonDown(1) && attackParryLockTimer <= 0f && parryCooldownTimer <= 0f && !m_rolling && !isParryActive)
            {
                m_animator.ResetTrigger("Block");
                m_animator.SetTrigger("Parry");
            }

            // 구르기
            else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding && !isParryActive)
            {
                m_rolling = true;
                m_animator.SetTrigger("Roll");
                m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
            }

            // 점프
            else if (Input.GetKeyDown("space") && m_grounded && !m_rolling && !isParryActive)
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", false);
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
                m_groundSensor?.Disable(0.2f);
            }

            // 이동 / 대기
            else if (Mathf.Abs(inputX) > Mathf.Epsilon) { m_delayToIdle = 0.05f; m_animator.SetInteger("AnimState", 1); }
            else { m_delayToIdle -= dt; if (m_delayToIdle < 0) m_animator.SetInteger("AnimState", 0); }
        }

        // 패링 윈도우 중: 성공 체크
        if (isParryActive)
        {
            Vector2 checkPos = transform.position + Vector3.right * m_facingDirection * 1.0f;
            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.5f, LayerMask.GetMask("Enemy"));
            if (hit != null)
            {
                var enemyRoot = hit.transform.root.gameObject;
                Debug.Log("<color=lime>패링 성공! 적: " + enemyRoot.name + "</color>");
                OnParrySuccess(enemyRoot);
                isParryActive = false;
                parrySucceeded = true;
                m_animator.SetTrigger("CounterAttack");
            }
            parryTimer += dt;
        }
    }

    // --- 애니메이션 이벤트 ---
    void AE_SlideDust()
    {
        Vector3 spawnPosition = m_facingDirection == 1 ?
            (m_wallSensorR2 ? m_wallSensorR2.transform.position : transform.position) :
            (m_wallSensorL2 ? m_wallSensorL2.transform.position : transform.position);

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, transform.localRotation);
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    // --- 패링 관련 ---
    public void OpenParryWindow()
    {
        isParryActive = true;
        parryTimer = 0f;
        parryWindowOpened = true;
        parrySucceeded = false;
        Debug.Log("<color=cyan>패링 윈도우 오픈!</color>");
    }

    public void CloseParryWindow()
    {
        isParryActive = false;
        Debug.Log("<color=yellow>패링 윈도우 종료!</color>");

        if (parryWindowOpened && !parrySucceeded)
        {
            OnParryFail();
        }
        parryWindowOpened = false;
        parrySucceeded = false;
    }

    public void OnParrySuccess(GameObject enemy)
    {
        Debug.Log("<color=orange>패링 반격 데미지 적용!</color>");
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>() ?? enemy.GetComponentInParent<EnemyBase>();
        if (enemyBase != null) enemyBase.TakeDamage(10);
        parryCooldownTimer = PARRY_SUCCESS_COOLDOWN;
        ParrySequenceSystem.Instance?.Begin(enemy.transform);
    }

    private void OnParryFail()
    {
        Debug.Log("<color=#FF8888>패링 실패! 쿨다운/락/경직 적용</color>");
        attackParryLockTimer = PARRY_FAIL_LOCK;
        parryCooldownTimer = PARRY_FAIL_COOLDOWN;
        stunnedTimer = PARRY_FAIL_STUN;
    }

    public void GuardEnd() { isParryActive = false; }

    // --- 공격 판정 제어 ---
    public void EnableHitbox()
    {
        PlayerAttackHitbox hitbox = GetComponentInChildren<PlayerAttackHitbox>();
        if (hitbox != null) { hitbox.EnableHitbox(); Debug.Log("<color=green>플레이어 공격 판정 ON</color>"); }
    }
    public void DisableHitbox()
    {
        PlayerAttackHitbox hitbox = GetComponentInChildren<PlayerAttackHitbox>();
        if (hitbox != null) { hitbox.DisableHitbox(); Debug.Log("<color=red>플레이어 공격 판정 OFF</color>"); }
    }
}

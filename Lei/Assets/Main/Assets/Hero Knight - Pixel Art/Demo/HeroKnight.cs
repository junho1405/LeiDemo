using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HeroKnight : MonoBehaviour
{
    [Header("Move/Action")]
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    [Header("Parry")]
    [SerializeField] private float parryCooldown = 4f;          // 패링 쿨타임(초)
    [SerializeField] private int parryCounterDamage = 30;     // 패링 반격 시 HP 대미지
    [SerializeField] private int parryStaggerDamage = 50;     // 패링 성공 시 경직도 감소량

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

    // 패링 상태
    private bool isParryActive = false;
    private float parryTimer = 0f;
    private float parryCooldownRemain = 0f;

    // 외부 접근
    public bool IsParryActive => isParryActive;
    public float ParryCooldownRemaining => Mathf.Max(0f, parryCooldownRemain);

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 컴포넌트
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1")?.GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2")?.GetComponent<Sensor_HeroKnight>();

        // 씬 로드 이벤트
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이동 시 센서 짧게 초기화
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
        // 타이머
        m_timeSinceAttack += Time.deltaTime;
        if (m_rolling) m_rollCurrentTime += Time.deltaTime;
        if (m_rollCurrentTime > m_rollDuration) m_rolling = false;

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

        // 이동 입력
        float inputX = Input.GetAxis("Horizontal");
        if (inputX > 0f)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0f)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // 이동 속도 적용(구르기 중엔 X속도 잠금)
        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        // 애니 파라미터
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
        m_isWallSliding =
            (m_wallSensorR1 != null && m_wallSensorR2 != null && m_wallSensorR1.State() && m_wallSensorR2.State()) ||
            (m_wallSensorL1 != null && m_wallSensorL2 != null && m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        // ===== 입력 처리 =====
        // 강제 죽음/피격 테스트
        if (Input.GetKeyDown(KeyCode.E) && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
        else if (Input.GetKeyDown(KeyCode.Q) && !m_rolling)
        {
            m_animator.SetTrigger("Hurt");
        }

        // 공격 (좌클릭)
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling && !isParryActive)
        {
            m_currentAttack++;
            if (m_currentAttack > 3) m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f) m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
        }

        // 패링 (우클릭) — 쿨타임 체크 추가
        else if (Input.GetMouseButtonDown(1) && !m_rolling && !isParryActive && parryCooldownRemain <= 0f)
        {
            m_animator.ResetTrigger("Block");
            m_animator.SetTrigger("Parry");
            parryCooldownRemain = parryCooldown; // 시도 시점에 쿨타임 시작
        }

        // 구르기 (Shift)
        else if (Input.GetKeyDown(KeyCode.LeftShift) && !m_rolling && !m_isWallSliding && !isParryActive)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }

        // 점프 (Space)
        else if (Input.GetKeyDown(KeyCode.Space) && m_grounded && !m_rolling && !isParryActive)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", false);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor?.Disable(0.2f);
        }

        // 이동/대기 애니 상태
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0f)
                m_animator.SetInteger("AnimState", 0);
        }

        // ===== 패링 감지(자동 성공 제거) =====
        if (isParryActive)
        {
            // 과거 OverlapCircle로 적이 근처면 즉시 성공 처리하던 코드 제거함.
            // 이제는 적의 EnemyAttackHitbox가 Enable된 상태에서만
            // EnemyAttackHitbox.OnTriggerEnter2D 쪽에서 성공 처리하도록 유지.
            parryTimer += Time.deltaTime;
        }

        // 패링 쿨타임 감소
        if (parryCooldownRemain > 0f)
            parryCooldownRemain -= Time.deltaTime;
    }

    // ===== 애니메이션 이벤트 =====
    void AE_SlideDust()
    {
        if (m_wallSensorR2 == null || m_wallSensorL2 == null) return;

        Vector3 spawnPosition = (m_facingDirection == 1)
            ? m_wallSensorR2.transform.position
            : m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, transform.localRotation);
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    // ===== 패링 윈도우 제어(애니 이벤트로 열고 닫음) =====
    public void OpenParryWindow()
    {
        isParryActive = true;
        parryTimer = 0f;
        Debug.Log("<color=cyan>패링 윈도우 오픈!</color>");
    }

    public void CloseParryWindow()
    {
        isParryActive = false;
        Debug.Log("<color=yellow>패링 윈도우 종료!</color>");
    }

    public void GuardEnd()
    {
        isParryActive = false;
    }

    // ===== 패링 성공 처리 (EnemyAttackHitbox에서 호출) =====
    public void OnParrySuccess(GameObject enemyGO)
    {
        var enemy = enemyGO ? enemyGO.GetComponentInParent<EnemyBase>() : null;
        if (enemy == null) return;

        Debug.Log("<color=orange>패링 반격 데미지 & 경직도 감소 적용!</color>");

        // 1) 반격 대미지(HP)
        enemy.TakeDamage(parryCounterDamage);

        // 2) 경직도는 패링 성공시에만 감소
        enemy.ReduceStagger(parryStaggerDamage);

        // 선택: 패링 시퀀스(쓰는 경우에만)
        ParrySequenceSystem.Instance?.Begin(enemy.transform);

        // 플레이어 반격 연출
        isParryActive = false;
        m_animator.SetTrigger("CounterAttack");
    }

    // ===== 플레이어 공격 판정 토글(애니 이벤트) =====
    public void EnableHitbox()
    {
        PlayerAttackHitbox hitbox = GetComponentInChildren<PlayerAttackHitbox>();
        if (hitbox != null)
        {
            hitbox.EnableHitbox();
            Debug.Log("<color=green>플레이어 공격 판정 ON</color>");
        }
    }

    public void DisableHitbox()
    {
        PlayerAttackHitbox hitbox = GetComponentInChildren<PlayerAttackHitbox>();
        if (hitbox != null)
        {
            hitbox.DisableHitbox();
            Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
        }
    }
}

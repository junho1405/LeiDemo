using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

[DisallowMultipleComponent]
public class FeverSequenceSystem : MonoBehaviour
{
    public static FeverSequenceSystem Instance { get; private set; }

    [Header("Sequence Settings")]
    [Tooltip("화면에 유지되는 키 개수")]
    public int sequenceSize = 5;

    [Tooltip("각 입력 단위 제한 시간(초)")]
    public float stepTime = 0.9f;

    [Tooltip("정답 1회 입력 성공 시 들어가는 피해량")]
    public int feverHitDamage = 30;

    [Tooltip("시퀀스에 사용되는 키 풀")]
    public KeyCode[] keyPool = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [Header("UI")]
    [Tooltip("지정 시 해당 폰트를 사용. 비우면 LegacyRuntime.ttf 사용")]
    public Font overrideFont;
    public Color accent = new Color(1f, 0.9f, 0.2f);

    // --- 내부 상태 ---
    private EnemyBase currentTarget;

    // ✅ 전체 피버 타이머(10초 등) — 시작 시 고정, 절대 리셋되지 않음
    private float feverTotal = 0f;
    private float feverRemain = 0f;

    private bool running = false;

    // 왼쪽부터만 유효 입력
    private readonly List<KeyCode> seq = new List<KeyCode>();

    // 스텝(키 하나) 제한시간 — 성공 시에만 리셋. 전체 타이머와는 별개
    private float stepTimer = 0f;

    // --- UI ---
    private Canvas canvas;
    private Text titleText;
    private Text seqText;

    // ✅ 이 바는 "전체 피버 남은 시간"만 표시한다
    private Image timerBar;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        EnsureUI();
        HideUI();
    }

    // =============================
    // Begin / Stop
    // =============================
    public void BeginFever(EnemyBase target, float durationSeconds)
    {
        if (target == null) return;

        currentTarget = target;

        // ✅ 전체 타이머 고정값 저장(예: 10초). 이후 절대 리셋하지 않음
        feverTotal = Mathf.Max(0.1f, durationSeconds);
        feverRemain = feverTotal;

        running = true;

        BuildInitialSequence();
        stepTimer = stepTime;

        // 게임 정지, UI는 unscaled 로 동작
        Time.timeScale = 0f;

        EnsureUI();
        ShowUI();
        RenderSequence();
        UpdateTimerBar(1f); // 시작 시 가득찬 상태

        Debug.Log("<color=#FFD700>[FEVER] 시작</color>");
    }

    private void StopFever(bool byTimeout = false)
    {
        running = false;
        Time.timeScale = 1f;

        if (currentTarget != null)
            currentTarget.ExitFeverTime();

        HideUI();
        currentTarget = null;
        seq.Clear();

        if (byTimeout)
            Debug.Log("<color=#00FFAA>[FEVER] 종료(시간만료)</color>");
        else
            Debug.Log("<color=#00FFAA>[FEVER] 종료</color>");
    }

    // 외부(EnemyBase)에서 적 사망 시 강제 종료용
    public void ForceStopFeverBy(EnemyBase target)
    {
        if (!running) return;
        if (currentTarget != target) return;
        StopFever();
    }

    void Update()
    {
        if (!running) return;

        // ✅ 전체 피버 잔여 시간(절대 리셋 없음)
        feverRemain -= Time.unscaledDeltaTime;
        if (feverRemain <= 0f)
        {
            StopFever(true);
            return;
        }

        // ✅ 전체 타이머 바를 갱신(시퀀스 성공과 무관하게 감소)
        float tRemain = (feverTotal > 0f) ? Mathf.Clamp01(feverRemain / feverTotal) : 0f;
        UpdateTimerBar(tRemain);

        // 스텝 제한시간(성공 시에만 리셋)
        stepTimer -= Time.unscaledDeltaTime;

        // 입력 처리: 반드시 '왼쪽(첫 요소)'만 정답. 틀린 키는 '무시'
        if (Input.anyKeyDown && seq.Count > 0)
        {
            // 키풀 내에서만 판정
            KeyCode pressed = KeyCode.None;
            for (int i = 0; i < keyPool.Length; i++)
            {
                if (Input.GetKeyDown(keyPool[i]))
                {
                    pressed = keyPool[i];
                    break;
                }
            }

            if (pressed != KeyCode.None)
            {
                KeyCode need = seq[0]; // 항상 첫 요소가 정답

                if (pressed == need)
                {
                    OnStepSuccess(); // 성공 시 스텝 타이머만 리셋
                }
                else
                {
                    // 실패: 무시(대미지/삭제/리셋 없음). 전체 타이머는 계속 흐름.
                }
            }
        }

        // 스텝 타이머가 0 이하가 되면, 요구사항에 따라 동작 결정
        // - 현재 정책: "페널티 없이 계속 시도 가능"을 원하면, 아래 리셋만 수행(피버 유지)
        // - 만약 스텝 타임아웃으로 피버를 끊고 싶다면 StopFever() 호출로 교체
        if (stepTimer <= 0f)
        {
            // 스텝 타임아웃 → 커서를 강제로 초기화하지 않고, 다시 입력할 수 있도록 스텝 타이머만 재충전
            stepTimer = stepTime;
        }
    }

    private void OnStepSuccess()
    {
        // 데미지 적용
        if (currentTarget != null)
            currentTarget.TakeFeverDamage(feverHitDamage);

        // 시퀀스: 첫 요소 제거 후, 맨 뒤에 랜덤 추가
        if (seq.Count > 0) seq.RemoveAt(0);
        seq.Add(RandomKey());

        // ✅ 성공 시에는 스텝 타이머만 리셋 (전체 피버 타이머는 리셋하지 않음)
        stepTimer = stepTime;

        RenderSequence();
    }

    // =============================
    // 시퀀스 빌드
    // =============================
    private void BuildInitialSequence()
    {
        seq.Clear();
        int len = Mathf.Max(1, sequenceSize);
        for (int i = 0; i < len; i++)
            seq.Add(RandomKey());
    }

    private KeyCode RandomKey()
    {
        return keyPool[Random.Range(0, keyPool.Length)];
    }

    // =============================
    // UI
    // =============================
    private void EnsureUI()
    {
        if (canvas != null) return;

        GameObject c = new GameObject("FeverCanvas");
        DontDestroyOnLoad(c);

        canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = c.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        c.AddComponent<GraphicRaycaster>();

        Font f = overrideFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        titleText = CreateText("FeverTitle", canvas.transform, new Vector2(0.5f, 0.82f), 32, TextAnchor.MiddleCenter, f);
        titleText.text = "FEVER TIME";
        titleText.color = accent;

        seqText = CreateText("Sequence", canvas.transform, new Vector2(0.5f, 0.68f), 28, TextAnchor.MiddleCenter, f);

        GameObject bar = new GameObject("TimerBar");
        bar.transform.SetParent(canvas.transform, false);
        timerBar = bar.AddComponent<Image>();
        timerBar.color = accent;

        var rt = bar.GetComponent<RectTransform>();
        // 화면 하단보다 약간 위쪽(기존 위치 유지)
        rt.anchorMin = new Vector2(0.25f, 0.60f);
        rt.anchorMax = new Vector2(0.75f, 0.63f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchor, int size, TextAnchor align, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var txt = go.AddComponent<Text>();
        txt.raycastTarget = false;
        txt.font = font;
        txt.fontSize = size;
        txt.alignment = align;
        txt.color = Color.white;

        var rt = txt.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(720, 80);

        return txt;
    }

    private void ShowUI() { if (canvas != null) canvas.enabled = true; }
    private void HideUI() { if (canvas != null) canvas.enabled = false; }

    private void RenderSequence()
    {
        if (seqText == null || seq == null) return;

        var sb = new StringBuilder();
        for (int i = 0; i < seq.Count; i++)
        {
            string k = seq[i].ToString();
            if (i == 0) sb.Append("[").Append(k).Append("]  "); // 맨 왼쪽(필수 입력) 강조
            else sb.Append(k).Append("  ");
        }
        seqText.text = sb.ToString();
    }

    // ✅ 전체 타이머 바(피버 잔여 시간)만 갱신
    private void UpdateTimerBar(float t01)
    {
        if (timerBar == null) return;
        t01 = Mathf.Clamp01(t01);

        var rt = timerBar.rectTransform;
        float min = 0.25f, max = 0.75f;
        float x = Mathf.Lerp(min, max, t01);
        rt.anchorMin = new Vector2(min, rt.anchorMin.y);
        rt.anchorMax = new Vector2(x, rt.anchorMax.y);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}

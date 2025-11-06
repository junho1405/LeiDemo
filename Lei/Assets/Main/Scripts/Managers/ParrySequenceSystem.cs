using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ParrySequenceSystem : MonoBehaviour
{
    public static ParrySequenceSystem Instance { get; private set; }

    [Header("Sequence Settings")]
    public int sequenceLength = 4;
    public float stepTime = 0.8f;
    public int successDamage = 40;
    public float staggerDamage = 25f;   // ✅ 패링 성공 시 경직도 감소량
    public KeyCode[] pool = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [Header("UI")]
    [Tooltip("지정 시 이 폰트를 사용합니다. 비워두면 LegacyRuntime.ttf 빌트인을 사용합니다.")]
    public Font overrideFont;

    // 내부 상태
    private readonly List<KeyCode> _seq = new List<KeyCode>();
    private int _cursor = 0;
    private float _timer = 0f;
    private bool _running = false;
    private Transform _currentTarget;

    // UI
    private Canvas _canvas;
    private Text _title;
    private Text _sequence;
    private Image _timerBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        EnsureUI();
        HideUI();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!_running) return;

        _timer -= Time.unscaledDeltaTime;
        UpdateTimerBar(Mathf.Clamp01(_timer / stepTime));

        if (_timer <= 0f)
        {
            StopSequence(false);
            return;
        }

        if (Input.anyKeyDown && _seq.Count > 0 && _cursor < _seq.Count)
        {
            if (Input.GetKeyDown(_seq[_cursor]))
            {
                _cursor++;
                RenderSequenceProgress();
                _timer = stepTime;

                if (_cursor >= _seq.Count)
                {
                    StopSequence(true);
                }
            }
            else
            {
                StopSequence(false);
            }
        }
    }

    // ---------------------------
    // ✅ 시퀀스 시작
    // ---------------------------
    public void Begin(Transform target, int length = -1)
    {
        if (_running) StopSequence(false);

        EnsureUI(); // 혹시 생성 실패했으면 재보장

        _currentTarget = target;
        int len = (length > 0) ? length : sequenceLength;

        BuildSequence(len);
        _cursor = 0;
        _timer = stepTime;
        _running = true;

        Time.timeScale = 0f;

        ShowUI();
        RenderSequenceProgress();
        UpdateTimerBar(1f);
    }

    public void ForceStop()
    {
        if (_running) StopSequence(false);
    }

    // ---------------------------
    // ✅ 시퀀스 종료 (성공/실패)
    // ---------------------------
    private void StopSequence(bool success)
    {
        _running = false;
        HideUI();
        Time.timeScale = 1f;

        if (success && _currentTarget != null)
        {
            var enemy = _currentTarget.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // ✅ 패링 성공 → 트루데미지 + 경직도 감소
                enemy.TakeParryDamage(successDamage, staggerDamage);

                Debug.Log($"<color=#00E5FF>[PARRY SUCCESS] Damage {successDamage}, Stagger {staggerDamage}</color>");
            }
        }

        _currentTarget = null;
        _seq.Clear();
    }

    // ---------------------------
    // ✅ 랜덤 입력 시퀀스 생성
    // ---------------------------
    private void BuildSequence(int len)
    {
        _seq.Clear();
        for (int i = 0; i < len; i++)
        {
            _seq.Add(pool[Random.Range(0, pool.Length)]);
        }
    }

    // =============================
    // ✅ UI 시스템
    // =============================
    private void EnsureUI()
    {
        if (_canvas != null && _title != null && _sequence != null && _timerBar != null)
            return;

        // Canvas 생성
        if (_canvas == null)
        {
            var c = new GameObject("ParrySequenceCanvas");
            c.transform.SetParent(null);
            _canvas = c.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = c.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            c.AddComponent<GraphicRaycaster>();

            DontDestroyOnLoad(c);
        }

        // ✅ Font 확보
        Font fontToUse = overrideFont;
        if (fontToUse == null)
            fontToUse = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Title
        if (_title == null)
        {
            _title = CreateText("Title", _canvas.transform, new Vector2(0.5f, 0.8f), 28, TextAnchor.MiddleCenter, fontToUse);
            _title.text = "PARRY SEQUENCE";
        }

        // Sequence 표시
        if (_sequence == null)
        {
            _sequence = CreateText("Sequence", _canvas.transform, new Vector2(0.5f, 0.7f), 24, TextAnchor.MiddleCenter, fontToUse);
        }

        // Timer Bar
        if (_timerBar == null)
        {
            GameObject bar = new GameObject("TimerBar");
            bar.transform.SetParent(_canvas.transform, false);
            _timerBar = bar.AddComponent<Image>();
            _timerBar.raycastTarget = false;

            var rect = _timerBar.rectTransform;
            rect.anchorMin = new Vector2(0.25f, 0.65f);
            rect.anchorMax = new Vector2(0.75f, 0.67f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private Text CreateText(string name, Transform parent, Vector2 anchor, int fontSize, TextAnchor align, Font font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var txt = go.AddComponent<Text>();
        txt.raycastTarget = false;
        txt.font = font;
        txt.fontSize = fontSize;
        txt.alignment = align;

        var rt = txt.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(700, 64);
        return txt;
    }

    private void ShowUI()
    {
        if (_canvas != null) _canvas.enabled = true;
    }

    private void HideUI()
    {
        if (_canvas != null) _canvas.enabled = false;
    }

    // ---------------------------
    // ✅ 현재 진행 상황 표시
    // ---------------------------
    private void RenderSequenceProgress()
    {
        if (_sequence == null || _seq == null) return;

        var sb = new StringBuilder();
        for (int i = 0; i < _seq.Count; i++)
        {
            bool done = i < _cursor;
            bool current = i == _cursor;
            string key = _seq[i].ToString();

            if (done) sb.Append("(").Append(key).Append(")  ");
            else if (current) sb.Append("[").Append(key).Append("]  ");
            else sb.Append(" ").Append(key).Append("   ");
        }
        _sequence.text = sb.ToString();
    }

    // ---------------------------
    // ✅ 타이머 바 업데이트
    // ---------------------------
    private void UpdateTimerBar(float t01)
    {
        if (_timerBar == null) return;
        var rt = _timerBar.rectTransform;
        float minX = 0.25f;
        float maxX = 0.75f;
        float x = Mathf.Lerp(minX, maxX, t01);

        rt.anchorMin = new Vector2(minX, rt.anchorMin.y);
        rt.anchorMax = new Vector2(x, rt.anchorMax.y);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}

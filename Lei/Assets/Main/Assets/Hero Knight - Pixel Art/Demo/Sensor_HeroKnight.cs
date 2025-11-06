using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sensor_HeroKnight : MonoBehaviour
{
    [Header("Detection Filter")]
    [Tooltip("이 센서가 인식할 레이어만 체크하세요 (예: Ground).")]
    public LayerMask detectLayers = ~0;

    [Tooltip("상대 콜라이더가 isTrigger이면 무시할지 여부")]
    public bool ignoreTriggerColliders = true;

    // 현재 실제로 겹쳐 있는 콜라이더 집합(중복 방지)
    private readonly HashSet<Collider2D> _overlaps = new HashSet<Collider2D>();

    // 일시 비활성화 타이머
    private float m_DisableTimer;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnEnable()
    {
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    private void OnDisable()
    {
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    public bool State()
    {
        if (m_DisableTimer > 0f) return false;
        return _overlaps.Count > 0;
    }

    private bool PassesFilter(Collider2D other)
    {
        // 1) 레이어 필터
        int otherLayer = other.gameObject.layer;
        bool layerOK = (detectLayers.value & (1 << otherLayer)) != 0;

        if (!layerOK) return false;

        // 2) 트리거 무시 옵션
        if (ignoreTriggerColliders && other.isTrigger) return false;

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PassesFilter(other)) return;
        _overlaps.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PassesFilter(other))
        {
            // 필터 바깥으로 바뀐 경우라도, 혹시 집합에 남아있으면 제거
            _overlaps.Remove(other);
            return;
        }
        _overlaps.Remove(other);
    }

    // 선택사항: 트리거 유지 중 필터가 바뀌는 경우를 보강
    private void OnTriggerStay2D(Collider2D other)
    {
        // DisableTimer 중에는 강제로 비움
        if (m_DisableTimer > 0f)
        {
            _overlaps.Clear();
            return;
        }

        if (PassesFilter(other))
            _overlaps.Add(other);
        else
            _overlaps.Remove(other);
    }

    private void Update()
    {
        if (m_DisableTimer > 0f)
            m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}

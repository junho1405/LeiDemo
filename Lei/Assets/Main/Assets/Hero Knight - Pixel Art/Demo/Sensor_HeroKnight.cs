using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sensor_HeroKnight : MonoBehaviour
{
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
        // 활성화될 때 안전 초기화
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    private void OnDisable()
    {
        // 비활성 시에도 누수 방지
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 시 이전 씬의 겹침 상태를 깨끗하게 리셋
        _overlaps.Clear();
        m_DisableTimer = 0f;
        // 필요하면 짧게 비활성(텔레포트 프레임 경계 보호)
        // m_DisableTimer = 0.05f;
        // Debug.Log($"[Sensor] SceneLoaded: {scene.name} -> reset overlaps");
    }

    public bool State()
    {
        if (m_DisableTimer > 0f) return false;
        return _overlaps.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 실제로 처음 겹치는 대상만 추가
        _overlaps.Add(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 실제로 겹침이 끝났을 때만 제거
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

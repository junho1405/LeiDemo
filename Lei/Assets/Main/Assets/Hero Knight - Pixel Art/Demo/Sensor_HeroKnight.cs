using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sensor_HeroKnight : MonoBehaviour
{
    private readonly HashSet<Collider2D> _overlaps = new HashSet<Collider2D>();
    private float m_DisableTimer;

    private void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnEnable() { _overlaps.Clear(); m_DisableTimer = 0f; }
    private void OnDisable() { _overlaps.Clear(); m_DisableTimer = 0f; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _overlaps.Clear();
        m_DisableTimer = 0f;
    }

    public bool State() => m_DisableTimer <= 0f && _overlaps.Count > 0;

    private void OnTriggerEnter2D(Collider2D other) { _overlaps.Add(other); }
    private void OnTriggerExit2D(Collider2D other) { _overlaps.Remove(other); }

    private void Update() { if (m_DisableTimer > 0f) m_DisableTimer -= Time.deltaTime; }

    public void Disable(float duration) { m_DisableTimer = duration; }
}

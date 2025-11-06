using UnityEngine;

[DisallowMultipleComponent]
public class PlayerParryBridge : MonoBehaviour
{
    [Header("안전한 자동 종료(ms) - 이벤트 누락 대비")]
    [SerializeField] private float fallbackWindow = 0.25f;

    public bool IsWindowActive { get; private set; }
    private float _autoCloseTime;

    private void Update()
    {
        if (IsWindowActive && _autoCloseTime > 0f && Time.time >= _autoCloseTime)
        {
            IsWindowActive = false;
            _autoCloseTime = 0f;
        }
    }

    // === HeroKnight 애니메이션 이벤트에서 호출 ===
    public void OpenParryWindow()
    {
        IsWindowActive = true;
        _autoCloseTime = Time.time + fallbackWindow;
        Debug.Log("<color=#00E5FF>[Parry] Window OPEN</color>");
    }

    // === HeroKnight 애니메이션 이벤트에서 호출 ===
    public void CloseParryWindow()
    {
        IsWindowActive = false;
        _autoCloseTime = 0f;
        Debug.Log("<color=#00E5FF>[Parry] Window CLOSE</color>");
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStats : MonoBehaviour
{
    [Header("HP (Fallback)")]
    public float maxHP = 100f;      // EnemyHealth 없을 때만 체력바가 이 값을 읽음
    public float currentHP = 100f;

    [Header("Stagger")]
    public float maxStagger = 30f;
    public float currentStagger = 30f;

    private void Awake()
    {
        if (maxHP <= 0f) maxHP = 1f;
        currentHP = Mathf.Clamp(currentHP <= 0f ? maxHP : currentHP, 0f, maxHP);

        if (maxStagger < 0f) maxStagger = 0f;
        currentStagger = Mathf.Clamp(currentStagger < 0f ? maxStagger : currentStagger, 0f, maxStagger);
    }

    public void ReduceStagger(float value)
    {
        if (maxStagger <= 0f) return;
        float v = Mathf.Abs(value);
        float before = currentStagger;
        currentStagger = Mathf.Max(0f, currentStagger - v);
        if (!Mathf.Approximately(before, currentStagger))
            Debug.Log($"<color=#FFA500>[Stagger] -{v} => {currentStagger}/{maxStagger}</color>");
    }

    public void ResetStagger()
    {
        currentStagger = maxStagger;
    }
}

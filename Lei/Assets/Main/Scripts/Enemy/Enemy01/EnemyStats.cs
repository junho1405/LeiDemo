using UnityEngine;

[DisallowMultipleComponent]
public class EnemyStats : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;
    public int currentHP = 100;

    [Header("Stagger")]
    public float maxStagger = 30f;
    public float currentStagger = 30f;

    [Header("Fever Flag")]
    public bool isInFever = false;

    EnemyBase owner;

    void Awake()
    {
        owner = GetComponent<EnemyBase>();
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        currentStagger = Mathf.Clamp(currentStagger, 0, maxStagger);
    }

    // 필요시 외부에서 호출
    public void ReduceStagger(float v)
    {
        currentStagger = Mathf.Max(0, currentStagger - Mathf.Abs(v));
        if (currentStagger <= 0f && !isInFever && owner != null)
            owner.TriggerFeverMode();
    }

    public void HealStagger(float v)
    {
        currentStagger = Mathf.Min(maxStagger, currentStagger + Mathf.Abs(v));
    }
}

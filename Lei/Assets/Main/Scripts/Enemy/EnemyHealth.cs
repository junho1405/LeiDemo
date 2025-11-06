using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (maxHP <= 0) maxHP = 1;
        currentHP = Mathf.Clamp(currentHP <= 0 ? maxHP : currentHP, 0, maxHP);
        IsDead = false;
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        int before = currentHP;
        currentHP = Mathf.Max(0, currentHP - Mathf.Max(0, dmg));
        if (currentHP != before)
            Debug.Log($"<color=#FF5555>[EnemyHealth] -{dmg} => {currentHP}/{maxHP}</color>");

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + Mathf.Max(0, amount));
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;
        Debug.Log("<color=red>[EnemyHealth] Dead</color>");
        // 여기서는 애니메이션/비활성화는 EnemyBase에서 처리
    }
}

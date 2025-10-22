using UnityEngine;

[System.Serializable]
public class EnemyStats
{
    public int maxHP = 100;
    public int currentHP;
    public int atk = 5;
    public int def = 0;
    public float moveSpeed = 2.5f;

    public void Init()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.Max(1, dmg - def);
        currentHP -= finalDamage;
        Debug.Log($"[Enemy] 피해량 {finalDamage} → 남은 HP {currentHP}");
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}

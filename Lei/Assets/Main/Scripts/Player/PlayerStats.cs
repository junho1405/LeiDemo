using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public int maxHP = 100;
    public int currentHP;
    public int def = 5;

    public void Init() { currentHP = maxHP; }

    public void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.Max(1, dmg - def);
        currentHP -= finalDamage;
        Debug.Log($"[Player] 피해량 {finalDamage} → 남은 HP {currentHP}");
    }

    public bool IsDead() => currentHP <= 0;
}

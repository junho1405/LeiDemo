using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public int maxHP = 100;
    public int currentHP;
    public int attackPower = 20;

    public void Init()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0)
            currentHP = 0;
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}

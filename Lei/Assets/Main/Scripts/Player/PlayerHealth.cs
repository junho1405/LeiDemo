using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP;

    [Header("FX")]
    public GameObject hitEffect;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        Debug.Log($"<color=red>[Player] 피해 {dmg} → 남은 HP {currentHP}</color>");

        if (hitEffect)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("<color=red>[Player] 사망</color>");
        // TODO: GameOver UI, 리스폰 등 후처리 추가 예정
    }
}

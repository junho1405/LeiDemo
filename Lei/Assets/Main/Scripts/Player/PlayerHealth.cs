using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    public PlayerStats stats;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        stats = new PlayerStats();
        stats.Init();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        stats.TakeDamage(dmg);
        Debug.Log($"플레이어 피해: {dmg}, 남은 HP: {stats.currentHP}");

        anim.Play("HeroKnight_Hurt");

        if (stats.IsDead())
            Die();
    }

    void Die()
    {
        isDead = true;
        anim.Play("HeroKnight_Death");
        Debug.Log("플레이어 사망");
    }
}

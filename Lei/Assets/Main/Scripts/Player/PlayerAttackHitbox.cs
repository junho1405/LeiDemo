using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAttackHitbox : MonoBehaviour
{
    public int damage = 10;
    public BoxCollider2D boxCol;

    private void Awake()
    {
        if (boxCol == null) boxCol = GetComponent<BoxCollider2D>();
        boxCol.enabled = false;
        boxCol.isTrigger = true;
    }

    public void EnableHitbox()
    {
        boxCol.enabled = true;
        Debug.Log("<color=red>플레이어 공격 판정 ON</color>");
    }

    public void DisableHitbox()
    {
        boxCol.enabled = false;
        Debug.Log("<color=red>플레이어 공격 판정 OFF</color>");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!boxCol.enabled) return;
        if (!col.TryGetComponent<EnemyBase>(out var enemy)) return;

        enemy.TakeDamage(damage);
        Debug.Log($"<color=yellow>적 피격! → {enemy.name}</color>");
    }
}

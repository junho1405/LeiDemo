using UnityEngine;

public class AnimatorEventRelay : MonoBehaviour
{
    private EnemyAttackHitbox attackHitbox;

    void Start()
    {
        attackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���
    public void EnableHitbox()
    {
        attackHitbox?.EnableHitbox();
    }

    public void DisableHitbox()
    {
        attackHitbox?.DisableHitbox();
    }
}

using UnityEngine;

public class AnimatorEventRelay : MonoBehaviour
{
    private EnemyAttackHitbox attackHitbox;

    void Start()
    {
        attackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
    }

    // 애니메이션 이벤트에서 호출됨
    public void EnableHitbox()
    {
        attackHitbox?.EnableHitbox();
    }

    public void DisableHitbox()
    {
        attackHitbox?.DisableHitbox();
    }
}

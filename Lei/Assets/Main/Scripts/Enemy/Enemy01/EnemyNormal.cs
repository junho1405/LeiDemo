using UnityEngine;

[DisallowMultipleComponent]
public class EnemyNormal : EnemyBase
{
    [Header("Param Names Override")]
    [SerializeField] private string overrideAttackTrigger = "Attack1";
    [SerializeField] private string overrideRunBool = "isRunning";

    protected override void Awake()
    {
        base.Awake();
        if (!string.IsNullOrEmpty(overrideAttackTrigger)) attackTriggerName = overrideAttackTrigger;
        if (!string.IsNullOrEmpty(overrideRunBool)) runBoolName = overrideRunBool;
        // 여기서는 캐시하지 않음 (Base.Start에서 최종 캐시)
    }

    protected override void Start()
    {
        base.Start();
        // 혹시 프리팹에서 런타임에 이름을 바꿨다면 여기서도 한 번 더 안전하게
        RefreshAnimatorParamCache();
    }

    public override void EnableHitbox() { base.EnableHitbox(); }
    public override void DisableHitbox() { base.DisableHitbox(); }
    public override void EndAttack() { base.EndAttack(); }
}

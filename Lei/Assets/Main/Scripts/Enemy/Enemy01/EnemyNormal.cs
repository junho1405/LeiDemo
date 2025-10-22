using UnityEngine;

public class EnemyNormal : EnemyBase
{
    protected override void Start()
    {
        base.Start(); // 부모 Start() 호출 (FindPlayer 포함)
    }

    protected override void Update()
    {
        base.Update(); // 부모 이동/공격 로직 그대로 사용
    }
}

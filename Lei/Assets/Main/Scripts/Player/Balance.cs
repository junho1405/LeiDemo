using UnityEngine;

public static class Balance
{
    // === Parry (Guard) ===
    public const float PARRY_SUCCESS_COOLDOWN = 4.0f;  // 성공 후 패링 재사용 대기
    public const float PARRY_FAIL_COOLDOWN = 6.0f;  // 실패 후 패링 재사용 대기
    public const float PARRY_FAIL_LOCK = 4.0f;  // 실패 후 공격/패링 입력 잠금 시간
    public const float PARRY_FAIL_STUN = 0.26f; // 실패 후 짧은 경직

    // === Enemy Tier ===
    public enum EnemyTier { Normal, Elite, Boss }

    // 플레이어가 적을 때릴 때(적이 "받는" 피해 배율) — 숫자가 작을수록 단단함
    public static float IncomingDamageMultiplier(EnemyTier tier) => tier switch
    {
        EnemyTier.Normal => 1.00f,
        EnemyTier.Elite => 0.75f,
        EnemyTier.Boss => 0.50f,
        _ => 1.0f
    };

    // 적이 플레이어를 때릴 때(적이 "주는" 피해 배율) — 숫자가 클수록 아픔
    public static float OutgoingDamageMultiplier(EnemyTier tier) => tier switch
    {
        EnemyTier.Normal => 1.00f,
        EnemyTier.Elite => 1.25f,
        EnemyTier.Boss => 1.50f,
        _ => 1.0f
    };
}

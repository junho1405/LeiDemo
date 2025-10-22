using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Awake()
    {
        // 이미 HeroKnight가 존재한다면 새로 만들지 않음
        if (GameObject.Find("HeroKnight") == null)
        {
            GameObject player = Instantiate(playerPrefab);
            player.name = "HeroKnight";
            DontDestroyOnLoad(player);

            // 태그 지정 (혹시 프리팹에서 누락된 경우 대비)
            player.tag = "Player";

            // 플레이어 생성 후 모든 적에게 플레이어 전달
            EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                enemy.SetPlayer(player.transform);
            }
        }
    }
}

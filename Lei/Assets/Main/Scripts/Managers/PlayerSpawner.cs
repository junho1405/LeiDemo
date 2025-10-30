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

            //Unity 2023+ 호환 버전
            EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                enemy.SetPlayer(player.transform);
            }
        }
    }
}

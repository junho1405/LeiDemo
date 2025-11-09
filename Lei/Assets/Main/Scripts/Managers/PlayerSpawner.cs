using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Awake()
    {
        if (GameObject.Find("HeroKnight") == null)
        {
            GameObject player = Instantiate(playerPrefab);
            player.name = "HeroKnight";
            player.tag = "Player";
            DontDestroyOnLoad(player);

            var enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var e in enemies)
                e.SetPlayer(player.transform);   // ✅ EnemyBase에 SetPlayer 존재
        }
    }
}

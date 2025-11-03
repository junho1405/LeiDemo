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
            DontDestroyOnLoad(player);
            player.tag = "Player";

            var enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var enemy in enemies) enemy.SetPlayer(player.transform);
        }
    }
}

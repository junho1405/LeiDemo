using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;

    void Awake()
    {
        // �̹� HeroKnight�� �����Ѵٸ� ���� ������ ����
        if (GameObject.Find("HeroKnight") == null)
        {
            GameObject player = Instantiate(playerPrefab);
            player.name = "HeroKnight";
            DontDestroyOnLoad(player);

            // �±� ���� (Ȥ�� �����տ��� ������ ��� ���)
            player.tag = "Player";

            //Unity 2023+ ȣȯ ����
            EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                enemy.SetPlayer(player.transform);
            }
        }
    }
}

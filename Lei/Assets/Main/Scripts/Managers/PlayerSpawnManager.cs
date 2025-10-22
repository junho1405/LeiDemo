using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        // HeroKnight ã��
        GameObject player = GameObject.Find("HeroKnight");
        if (player == null) return;

        // ���� ���� ��ġ ����
        string spawnName = SceneTransition.Instance?.nextSpawnPoint;
        if (!string.IsNullOrEmpty(spawnName))
        {
            Transform spawn = GameObject.Find(spawnName)?.transform;
            if (spawn != null)
                player.transform.position = spawn.position;
        }
    }
}

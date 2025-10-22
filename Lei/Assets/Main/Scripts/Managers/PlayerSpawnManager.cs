using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        // HeroKnight 찾기
        GameObject player = GameObject.Find("HeroKnight");
        if (player == null) return;

        // 다음 스폰 위치 설정
        string spawnName = SceneTransition.Instance?.nextSpawnPoint;
        if (!string.IsNullOrEmpty(spawnName))
        {
            Transform spawn = GameObject.Find(spawnName)?.transform;
            if (spawn != null)
                player.transform.position = spawn.position;
        }
    }
}

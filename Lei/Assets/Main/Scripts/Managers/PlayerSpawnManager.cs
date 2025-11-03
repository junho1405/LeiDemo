using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.Find("HeroKnight");
        if (player == null) return;

        string spawnName = SceneTransition.Instance?.nextSpawnPoint;
        if (!string.IsNullOrEmpty(spawnName))
        {
            Transform spawn = GameObject.Find(spawnName)?.transform;
            if (spawn != null) player.transform.position = spawn.position;
        }
    }
}

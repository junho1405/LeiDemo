using UnityEngine;
using UnityEngine.SceneManagement;

public class StagePortal : MonoBehaviour
{
    [SerializeField] string targetScene;      // 이동할 씬 이름
    [SerializeField] string targetSpawnPoint; // 다음 씬에서의 스폰 위치명

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "HeroKnight")
        {
            SceneTransition.Instance.nextScene = targetScene;
            SceneTransition.Instance.nextSpawnPoint = targetSpawnPoint;
            SceneManager.LoadScene(targetScene);
        }
    }
}

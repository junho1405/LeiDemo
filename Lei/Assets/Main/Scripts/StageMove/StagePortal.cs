using UnityEngine;
using UnityEngine.SceneManagement;

public class StagePortal : MonoBehaviour
{
    [SerializeField] string targetScene;      // �̵��� �� �̸�
    [SerializeField] string targetSpawnPoint; // ���� �������� ���� ��ġ��

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

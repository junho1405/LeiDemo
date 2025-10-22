using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    public string nextSpawnPoint; // ���� ������ ������ ��ġ��
    public string nextScene;      // �̵��� �� �̸�

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

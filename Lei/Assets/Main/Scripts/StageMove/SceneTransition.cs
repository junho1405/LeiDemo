using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    public string nextSpawnPoint; // 다음 씬에서 등장할 위치명
    public string nextScene;      // 이동할 씬 이름

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

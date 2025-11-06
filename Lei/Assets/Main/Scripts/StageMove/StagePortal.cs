using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor; // SceneAsset 참조용(에디터에서만)
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class StagePortal : MonoBehaviour
{
    public enum SpawnSide { Default, Left, Right }

    [Header("Target Scene")]
    [Tooltip("빌드에서 로드할 씬 이름(런타임 사용). 확장자 없이 정확히!")]
    public string sceneName = "Stage2";

#if UNITY_EDITOR
    [Tooltip("에디터에서만 사용: 여기 SceneAsset을 드래그하면 sceneName이 자동으로 동기화됩니다.")]
    public SceneAsset sceneAsset;
#endif

    [Header("Spawn For NEXT Scene")]
    public SpawnSide nextSpawnSide = SpawnSide.Default;
    [Tooltip("우선순위 1: 직접 지정할 스폰 이름 (예: LeftSpawn / RightSpawn)")]
    public string overrideSpawnPointName = "";

    [Header("Player / Options")]
    public string playerTag = "Player";
    public bool additive = false;
    public int waitFrames = 10;

    // 내부 이동 컨텍스트
    private bool _pendingTravel = false;
    private string _pendingScene;
    private string _pendingSpawnName;
    private bool _pendingAdditive;
    private string _pendingPlayerTag;
    private int _pendingWaitFrames;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 SceneAsset을 채우면 sceneName을 자동 동기화
        if (sceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(sceneAsset);
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (asset != null)
            {
                // "Assets/.../Stage4.unity" → "Stage4"
                var n = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(n)) sceneName = n;
            }
        }
    }
#endif

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        var hit = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (hit == null || !hit.CompareTag(playerTag)) return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[StagePortal] sceneName 이 비어있습니다. 인스펙터에서 설정하세요.");
            return;
        }

        // 빌드 가능 여부 사전 확인
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[StagePortal] Scene '{sceneName}' 을(를) 로드할 수 없습니다. " +
                           "File → Build Settings에서 'Scenes In Build'에 추가하고 체크하세요. " +
                           "또는 인스펙터에서 씬 이름 오타/대소문자를 확인하세요.");
            return;
        }

        string spawnName = null;
        if (!string.IsNullOrWhiteSpace(overrideSpawnPointName))
        {
            spawnName = overrideSpawnPointName.Trim();
        }
        else
        {
            switch (nextSpawnSide)
            {
                case SpawnSide.Left: spawnName = "LeftSpawn"; break;
                case SpawnSide.Right: spawnName = "RightSpawn"; break;
                default: spawnName = null; break;
            }
        }

        _pendingTravel = true;
        _pendingScene = sceneName;
        _pendingSpawnName = spawnName;
        _pendingAdditive = additive;
        _pendingPlayerTag = playerTag;
        _pendingWaitFrames = Mathf.Max(0, waitFrames);

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        var mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        Debug.Log($"[StagePortal] Loading scene '{_pendingScene}', NextSpawn='{_pendingSpawnName ?? "(none)"}', mode={mode}");
        SceneManager.LoadScene(_pendingScene, mode);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_pendingTravel) return;
        StartCoroutine(PlacePlayerAfterLoad());
    }

    private IEnumerator PlacePlayerAfterLoad()
    {
        Transform player = null;
        Transform spawn = null;

        for (int f = 0; f <= _pendingWaitFrames; f++)
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(_pendingPlayerTag);
                if (go) player = go.transform;
            }

            if (spawn == null && !string.IsNullOrWhiteSpace(_pendingSpawnName))
            {
                var spGo = GameObject.Find(_pendingSpawnName);
                if (spGo) spawn = spGo.transform;
            }

            if (player != null && (spawn != null || string.IsNullOrWhiteSpace(_pendingSpawnName)))
                break;

            yield return null;
        }

        if (player != null && spawn != null)
        {
            player.position = spawn.position;
            Debug.Log($"[StagePortal] Player moved to '{spawn.name}' at {spawn.position}");
        }
        else if (player != null && string.IsNullOrWhiteSpace(_pendingSpawnName))
        {
            Debug.Log("[StagePortal] No spawn requested. Keeping player's current position.");
        }
        else
        {
            Debug.LogWarning($"[StagePortal] Spawn not placed. player={(player ? player.name : "null")}, requested='{_pendingSpawnName ?? "null"}'");
        }

        CleanupTraveler();
    }

    private void CleanupTraveler()
    {
        _pendingTravel = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Destroy(gameObject); // 잔류 방지
    }
}

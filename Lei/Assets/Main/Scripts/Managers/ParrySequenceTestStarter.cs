using System.Linq;
using UnityEngine;

public class ParrySequenceTestStarter : MonoBehaviour
{
    public KeyCode startKey = KeyCode.F5;

    void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            if (ParrySequenceSystem.Instance == null) return;

            var enemies = GameObject.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0) return;

            var player = GameObject.Find("HeroKnight");
            Vector3 p = player != null ? player.transform.position : Vector3.zero;

            var nearest = enemies
                .OrderBy(e => Vector3.SqrMagnitude(e.transform.position - p))
                .FirstOrDefault();

            if (nearest != null)
                ParrySequenceSystem.Instance.Begin(nearest.transform);
        }
    }
}

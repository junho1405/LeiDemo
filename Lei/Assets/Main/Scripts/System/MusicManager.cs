using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;  // Stage1에서 지정
    [SerializeField] private bool loop = true;
    [Range(0f, 1f)][SerializeField] private float volume = 0.6f;

    [Header("Fade")]
    [Tooltip("씬 시작 시 페이드인 시간(초)")]
    [SerializeField] private float fadeInSeconds = 0.5f;

    private AudioSource _src;

    private void Awake()
    {
        // 싱글톤: 중복 생성 방지
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 오디오소스 준비
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = loop;
        _src.volume = 0f;

        // Stage1에서만 클립 지정하고 자동 재생 (이후 씬에서도 유지)
        if (bgmClip != null)
            StartCoroutine(FadeInPlay(bgmClip, fadeInSeconds, volume));
    }

    // 외부에서 볼륨/정지 등을 제어하고 싶을 때 쓸 수 있는 정적 메서드들
    public static void SetVolume(float v)
    {
        if (_instance == null) return;
        _instance.volume = Mathf.Clamp01(v);
        if (_instance._src != null) _instance._src.volume = _instance.volume;
    }

    public static void Stop(float fadeOutSeconds = 0.5f)
    {
        if (_instance == null) return;
        _instance.StartCoroutine(_instance.FadeOutStop(fadeOutSeconds));
    }

    public static void PlayClip(AudioClip clip, float crossfadeSeconds = 0.5f, float targetVolume = 0.6f, bool loop = true)
    {
        if (_instance == null) return;
        _instance.loop = loop;
        _instance.volume = Mathf.Clamp01(targetVolume);
        _instance.StartCoroutine(_instance.CrossfadeTo(clip, crossfadeSeconds));
    }

    // ===== 내부 구현 =====

    private IEnumerator FadeInPlay(AudioClip clip, float fadeTime, float targetVol)
    {
        _src.clip = clip;
        _src.loop = loop;
        _src.volume = 0f;
        _src.Play();

        if (fadeTime <= 0f)
        {
            _src.volume = targetVol;
            yield break;
        }

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(0f, targetVol, t / fadeTime);
            yield return null;
        }
        _src.volume = targetVol;
    }

    private IEnumerator FadeOutStop(float fadeTime)
    {
        if (_src == null || !_src.isPlaying) yield break;

        float start = _src.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }

        _src.Stop();
        _src.volume = volume; // 다음 재생 대비
    }

    private IEnumerator CrossfadeTo(AudioClip next, float seconds)
    {
        if (next == null) yield break;

        // 같은 클립이면 무시
        if (_src.clip == next && _src.isPlaying) yield break;

        float startVol = _src.volume;
        float t = 0f;

        // 페이드아웃
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(startVol, 0f, t / seconds);
            yield return null;
        }

        // 교체 + 페이드인
        _src.clip = next;
        _src.loop = loop;
        _src.Play();

        t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            _src.volume = Mathf.Lerp(0f, volume, t / seconds);
            yield return null;
        }

        _src.volume = volume;
    }
}

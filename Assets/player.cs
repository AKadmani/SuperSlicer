using UnityEngine;
using System.Collections;

/// <summary>
/// Plays and fades background music across the whole game.
/// Put it on MusicPlayer and make sure the object exists in the first scene.
/// </summary>
public class player : MonoBehaviour
{
    public AudioSource source;          // assign in Inspector
    public float fadeSeconds = 1f;

    static player _instance;
    void Awake()
    {
        if (_instance) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayLoop()
    {
        if (source.isPlaying) return;
        source.loop = true;
        StartCoroutine(FadeIn());
    }

    public void Stop()
    {
        if (!source.isPlaying) return;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        source.volume = 0f;
        source.Play();
        for (float t = 0; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            source.volume = t / fadeSeconds;
            yield return null;
        }
        source.volume = 1f;
    }

    IEnumerator FadeOut()
    {
        float start = source.volume;
        for (float t = 0; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(start, 0f, t / fadeSeconds);
            yield return null;
        }
        source.Stop();
    }
}

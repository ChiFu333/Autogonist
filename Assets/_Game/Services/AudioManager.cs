using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, IService
{
    private AudioSource musicSource;
    private AudioSource soundsSource;

    public float musicVolume { get; private set; } = 0.5f;
    public float soundVolume { get; private set; } = 0.5f;
    
    private Dictionary<int, ActiveSoundLoop> _activeLoops = new Dictionary<int, ActiveSoundLoop>();
    private int _nextLoopId = 0;
    
    private class ActiveSoundLoop
    {
        public AudioSource Source;
        public Coroutine Coroutine;
    }
    
    public void Init(bool showWhenInit)
    {
        GameObject mSource = new GameObject("MusicSource")
        {
            transform =
            {
                parent = this.transform
            }
        };
        musicSource = mSource.AddComponent<AudioSource>();
        musicSource.loop = true;
        
        GameObject sSource = new GameObject("AudioSource")
        {
            transform =
            {
                parent = this.transform
            }
        };
        soundsSource = sSource.AddComponent<AudioSource>();
        soundsSource.loop = false;
        
        PlayMusic(R.Audio.MainMenu);
        
        SetMusicVolume(musicVolume);
        SetSoundVolume(soundVolume);
    }
    public void SetMusicVolume(float value) {
        musicVolume = value;
        musicSource.volume = musicVolume;
    }

    public void SetSoundVolume(float value) {
        soundVolume = value;
        soundsSource.volume = soundVolume;
    }

    public void PlaySound(AudioClip clip) {
        if (clip == null) return;
        soundsSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip) {
        musicSource.UnPause();
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void StopMusic() {
        musicSource.Pause();
    }
    public void PlayWithRandomPitch(AudioClip clip, float d)
    {
        if (clip == null) return;
        GameObject tempAudioObject = new GameObject("TempAudio_" + clip.name);
        DontDestroyOnLoad(tempAudioObject);
        AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = 1f + Random.Range(-d, d); // Случайный pitch в диапазоне
        audioSource.Play();

        // Уничтожаем объект после завершения воспроизведения
        Destroy(tempAudioObject, clip.length + 0.1f); // Небольшой запас на всякий случай
    }
    public int PlayLoop(AudioClip clip, float intervalSeconds)
    {
        if (clip == null) return -1;

        int id = _nextLoopId++;
        GameObject loopObject = new GameObject($"SoundLoop_{id}");
        loopObject.transform.parent = this.transform;

        AudioSource source = loopObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = soundVolume;
        source.loop = false;

        ActiveSoundLoop loop = new ActiveSoundLoop()
        {
            Source = source,
            Coroutine = StartCoroutine(SoundLoopRoutine(source, clip, intervalSeconds, id))
        };

        _activeLoops.Add(id, loop);
        return id;
    }

    private IEnumerator SoundLoopRoutine(AudioSource source, AudioClip clip, float interval, int id)
    {
        while (true)
        {
            PlayWithRandomPitch(clip, 0.25f);
            yield return new WaitForSeconds(interval + clip.length); // Интервал + длительность звука
        }
    }

    /// <summary>
    /// Останавливает луп-звук по ID.
    /// </summary>
    public void StopLoop(int id)
    {
        if (_activeLoops.TryGetValue(id, out ActiveSoundLoop loop))
        {
            if (loop.Coroutine != null)
                StopCoroutine(loop.Coroutine);

            if (loop.Source != null)
                Destroy(loop.Source.gameObject);

            _activeLoops.Remove(id);
        }
    }

    /// <summary>
    /// Останавливает все луп-звуки.
    /// </summary>
    public void StopAllLoops()
    {
        foreach (var loop in _activeLoops.Values)
        {
            if (loop.Coroutine != null)
                StopCoroutine(loop.Coroutine);

            if (loop.Source != null)
                Destroy(loop.Source.gameObject);
        }
        _activeLoops.Clear();
    }

    private void OnDestroy()
    {
        StopAllLoops(); // Очистка при уничтожении
    }
}

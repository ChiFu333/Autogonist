using UnityEngine;
using System.Collections;

public class SoundLooper : MonoBehaviour
{
    private AudioSource _audioSource;
    private Coroutine _soundLoopCoroutine;
    private bool _isPlaying;
    
    public void PlayLoop(AudioClip audioClip, float intervalSeconds, float volume = 1f)
    {
        if (_isPlaying) 
        {
            Debug.LogWarning("SoundLooper уже воспроизводит звук!");
            return;
        }

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        _audioSource.clip = audioClip;
        _audioSource.volume = volume;
        _isPlaying = true;

        _soundLoopCoroutine = StartCoroutine(SoundLoopRoutine(intervalSeconds));
    }

    private IEnumerator SoundLoopRoutine(float intervalSeconds)
    {
        while (_isPlaying)
        {
            _audioSource.Play();
            yield return new WaitForSeconds(intervalSeconds);
        }
    }

    /// <summary>
    /// Останавливает воспроизведение звука.
    /// </summary>
    public void Stop()
    {
        if (!_isPlaying) return;

        _isPlaying = false;
        
        if (_soundLoopCoroutine != null)
        {
            StopCoroutine(_soundLoopCoroutine);
            _soundLoopCoroutine = null;
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    private void OnDestroy()
    {
        Stop(); // Автоматическая очистка при уничтожении объекта
    }
}
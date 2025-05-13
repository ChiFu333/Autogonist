using UnityEngine;
using DG.Tweening;

public class PressMachine : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _pressHead; // Прессующая головка
    [SerializeField] private float _workCycleTime = 3f; // Полный цикл работы
    [SerializeField] private float _pressDownDuration = 0.5f; // Время движения вниз
    [SerializeField] private float _pressUpDuration = 1f; // Время подъема
    [SerializeField] private float _bottomPositionY = 0.5f; // Нижняя точка пресса
    [SerializeField] private float _accelerationCurve = 2f; // Кривая ускорения

    [Header("Debug")]
    [SerializeField] private bool _isWorking = false;
    [SerializeField] private bool _isPaused = false;

    private Vector3 _originalPosition;
    private Sequence _pressSequence;
    private float _timer;

    private void Awake()
    {
        _originalPosition = _pressHead.position;
    }

    private void Update()
    {
        if (_isWorking && !_isPaused)
        {
            _timer += Time.deltaTime;
            if (_timer >= _workCycleTime)
            {
                _timer = 0f;
                StartPressCycle();
            }
        }
    }

    private void StartPressCycle()
    {
        // Отменяем предыдущую анимацию если была
        _pressSequence?.Kill();

        _pressSequence = DOTween.Sequence();
        
        // Движение вниз с ускорением
        G.AudioManager.PlaySound(R.Audio.press);
        _pressSequence.Append(
            _pressHead.DOLocalMoveY(_bottomPositionY, _pressDownDuration)
                .SetEase(Ease.InExpo) // Кривая ускорения
                .OnComplete(OnPressDownComplete)
        );
        
        // Движение вверх с постоянной скоростью
        _pressSequence.Append(
            _pressHead.DOMoveY(_originalPosition.y, _pressUpDuration)
                .SetEase(Ease.Linear)
        );

        _pressSequence.Play();
    }

    private void OnPressDownComplete()
    {
        // Здесь можно добавить логику обработки прессования
        
    }

    // === Управление станком ===
    public void StartMachine()
    {
        if (_isWorking) return;
        
        _isWorking = true;
        _isPaused = false;
        _timer = 0f;
        StartPressCycle(); // Начинаем сразу первый цикл
    }

    public void StopMachine()
    {
        _isWorking = false;
        _pressSequence?.Kill(); // Останавливаем анимацию
    }

    public void PauseMachine()
    {
        _isPaused = true;
        _pressSequence?.Pause();
    }

    public void ResumeMachine()
    {
        _isPaused = false;
        _pressSequence?.Play();
    }

    public void EmergencyStop()
    {
        StopMachine();
        // Быстро поднимаем пресс в исходное положение
        _pressHead.DOMoveY(_originalPosition.y, 0.5f)
            .SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        _pressSequence?.Kill();
    }
}
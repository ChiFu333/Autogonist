using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

public class AnimationController : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer _targetRenderer;
    private AnimationDataSO _currentAnimation;
    private CancellationTokenSource _cancellationTokenSource;

    private int frame = 0;

    public void Init()
    {
        _targetRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetAnimation(AnimationDataSO newAnimation)
    {
        if (_currentAnimation != newAnimation)
        {
            StopAnimation();
            
            _currentAnimation = newAnimation;
            
            _cancellationTokenSource = new CancellationTokenSource();
            Anim(_currentAnimation, _cancellationTokenSource.Token).Forget();
        }
    }
    public async UniTask SetAnimOneShot(AnimationDataSO newAnimation)
    {
        if (_currentAnimation != newAnimation)
        {
            StopAnimation();
            
            _currentAnimation = newAnimation;
            
            _cancellationTokenSource = new CancellationTokenSource();
            await PlayAnimOneShot(_currentAnimation, _cancellationTokenSource.Token);
        }
    }

    public void SetFlip(bool flip) => _targetRenderer.flipX = flip;

    private async UniTask Anim(AnimationDataSO myAnimData, CancellationToken cancellationToken)
    {
        frame = 0;
        while (_currentAnimation == myAnimData && !cancellationToken.IsCancellationRequested)
        {
            _targetRenderer.sprite = myAnimData.frames[frame];
            frame = (frame + 1) % _currentAnimation.frames.Count;
            await UniTask.Delay((int)(1000 / myAnimData.framerate), DelayType.Realtime, cancellationToken: cancellationToken);
        }
    }
    private async UniTask PlayAnimOneShot(AnimationDataSO myAnimData, CancellationToken cancellationToken)
    {
        frame = 0;
        bool firstZero = true;
        while (_currentAnimation == myAnimData && !cancellationToken.IsCancellationRequested)
        {
            _targetRenderer.sprite = myAnimData.frames[frame];
            frame = (frame + 1) % _currentAnimation.frames.Count;
            await UniTask.Delay((int)(1000 / myAnimData.framerate), DelayType.Realtime, cancellationToken: cancellationToken);
            if (frame == 0 && !firstZero)
            {
                break;
            }
            firstZero = false;
        }
    }

    private void StopAnimation()
    {
        // Отменяем текущую задачу, если она существует
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private void OnDestroy()
    {
        // Останавливаем анимацию при уничтожении объекта
        StopAnimation();
    }
}
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    public List<ScheduleLine> schedule;
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    private CancellationTokenSource movementCTS;
    
    [Header("Animations")] 
    [SerializeField] private AnimationDataSO standing;
    [SerializeField] private AnimationDataSO walking;
    [SerializeField] private AnimationDataSO goUpLift;
    public AnimationController _enemyAnim;

    public bool _inAnim;
    [HideInInspector] public bool end = false;

    public Room myRoom;
    private Elevator myElev;
    private Door myDoor;

    [Serializable] public class ScheduleLine
    {
        public GameObject place;
        public float timeToStay;
    }
    
    private void Awake()
    {
        _enemyAnim = GetComponentInChildren<AnimationController>();
        _enemyAnim.Init();
        _enemyAnim.SetAnimation(standing);
    }

    private async void Start()
    {
        _enemyAnim.SetAnimation(standing);
        ReadSchedule().Forget();
    }

    private async UniTask ReadSchedule()
    {
        while (true)
        {
            for (int i = 0; i < schedule.Count; i++)
            {
                if (schedule[i].place == null) return;
                if (end) return;
                if (schedule[i].place.TryGetComponent(out Elevator elevator))
                {
                    myElev = elevator;
                    await StartMovement(elevator.transform.position.x, OnReachedElevator);
                }
                else if (schedule[i].place.TryGetComponent(out Door door))
                {
                    float del = door.transform.position.x - transform.position.x;
                    myDoor = door;
                    await StartMovement(door.transform.position.x + ((del < 0) ? door.deltaX : -door.deltaX ), OnReachedDoor);
                }
                else
                {
                    await StartMovement(schedule[i].place.transform.position.x);
                    await UniTask.Delay((int)(schedule[i].timeToStay * 1000));
                }

                
            }
        }
    }
    public async UniTask StartMovement(float targetX, Func<UniTask> onComplete = null)
    {
        var newCTS = new CancellationTokenSource();

        CancelCurrentMovement();
        movementCTS = newCTS;
        await MoveToTargetAsync(targetX, onComplete, newCTS.Token);
    }
    private async UniTask MoveToTargetAsync(float targetX, Func<UniTask> onComplete, CancellationToken ct)
    {
        //int t = G.AudioManager.PlayLoop(R.Audio.step, 0.05f);
        try
        {
            bool b = transform.position.x - targetX > 0;
            _enemyAnim.SetFlip(b);
            _enemyAnim.SetAnimation(walking);
            
            // Движение к цели
            while (!ct.IsCancellationRequested && 
                   Mathf.Abs(transform.position.x - targetX) > stoppingDistance)
            {
                float newX = Mathf.MoveTowards(
                    transform.position.x, 
                    targetX, 
                    moveSpeed * Time.deltaTime
                );
                
                transform.position = new Vector3(
                    newX, 
                    transform.position.y, 
                    transform.position.z
                );

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Если движение не было отменено
            if (!ct.IsCancellationRequested)
            {
                //G.AudioManager.StopLoop(t);
                transform.position = new Vector3(
                    targetX, 
                    transform.position.y, 
                    transform.position.z
                );
                _enemyAnim.SetAnimation(standing);
                if (onComplete != null)
                {
                    await onComplete();
                }
            }
        }
        catch (OperationCanceledException)
        {
            //G.AudioManager.StopLoop(t);
            // Движение было отменено - ничего не делаем
        }
    }
    public void CancelCurrentMovement()
    {
        if (movementCTS != null)
        {
            // Отменяем только если не был disposed
            if (!movementCTS.IsCancellationRequested)
            {
                movementCTS.Cancel();
            }
            movementCTS.Dispose();
            movementCTS = null;
        }
    }
    private async UniTask OnReachedElevator()
    {
        float timeToGoUp = 0.4f;
        float deltaUp = 0.5f;
        myElev.myLiftHandler.isLockedByEnemy = true;

        while (!await myElev.myLiftHandler.CallElevator(myElev))
        {
            await UniTask.Delay(250);
        }
        _inAnim = true;
        //Go in lift
        _enemyAnim.SetAnimation(goUpLift);
        G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f);
        DOVirtual.DelayedCall(0.25f, () => { G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f); });
        await transform.GetChild(0).DOLocalMoveY(transform.GetChild(0).localPosition.y + deltaUp, timeToGoUp)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        _enemyAnim._targetRenderer.sortingOrder = 5;
        _enemyAnim.SetAnimation(standing);
        
        //CloseDoors and wait
        await myElev.myLiftHandler.TeleportPlayer(this, myElev);
        //GoOut
        _enemyAnim._targetRenderer.sortingOrder = 50;
        _enemyAnim.SetAnimation(standing);
        G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f);
        DOVirtual.DelayedCall(0.25f, () => { G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f); });
        await transform.GetChild(0).DOLocalMoveY(0, timeToGoUp)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        
        _enemyAnim.SetAnimation(standing);
        myElev.myLiftHandler.isLockedByEnemy = false;
        _inAnim = false;
    }
    private async UniTask OnReachedDoor()
    {
        float del = myDoor.transform.position.x - transform.position.x;
        
        _inAnim = true;
        
        await myDoor.OpenAnim();
        
        CancelCurrentMovement();
        var newCTS = new CancellationTokenSource();
        movementCTS = newCTS;
        await MoveToTargetAsync(myDoor.transform.position.x + ((del > 0) ? myDoor.deltaX * 1.2f : -myDoor.deltaX * 1.2f), null, newCTS.Token);
        myRoom = (del < 0) ? myDoor.left : myDoor.right;
        _ = myDoor.CloseAnim();
        myDoor.goEvent.Invoke();
        _inAnim = false;
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using DG.Tweening;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    private CancellationTokenSource movementCTS;
    
    public Room myRoom;
    
    private Elevator myElev;
    private Door myDoor;
    private Storage myStorage;
    private Breakable myBreakable;
    
    [Header("Animations")] 
    [SerializeField] private AnimationDataSO standing;
    [SerializeField] private AnimationDataSO walking;
    [SerializeField] private AnimationDataSO goUpLift;
    [SerializeField] private AnimationDataSO goDownLift;
    [SerializeField] private AnimationDataSO searching;
    private AnimationController playerAnim;
    
    public bool _inAnim = false;

    private void Awake()
    {
        playerAnim = GetComponentInChildren<AnimationController>();
        playerAnim.Init();
        playerAnim.SetAnimation(standing);
    }

    private void OnDestroy()
    {
        CancelCurrentMovement();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !_inAnim && !ScreenUtils.IsMouseOverUI())
        {
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(clickPos, Vector2.zero);
            
            Array.Sort(hits, (a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null) continue;
            
                if (hit.collider.TryGetComponent(out Elevator elevator) && elevator.myRoom == myRoom)
                {
                    if (elevator.myLiftHandler.isLockedByEnemy)
                    {
                        G.AudioManager.PlayWithRandomPitch(R.Audio.Wrong_Error,0.1f);
                        return;
                    }
                    myElev = elevator;
                    StartMovement(elevator.transform.position.x,OnReachedElevator);
                    break;
                }
                
                if (hit.collider.TryGetComponent(out Door door))
                {
                    float del = door.transform.position.x - transform.position.x;
                    if ((del > 0 && door.left == myRoom) || (del < 0 && door.right == myRoom))
                    {
                        if (door.isWorking == false)
                        {
                            G.AudioManager.PlayWithRandomPitch(R.Audio.Wrong_Error,0.1f);
                            return;
                        }
                        myDoor = door;
                        StartMovement(door.transform.position.x + ((del < 0) ? door.deltaX : -door.deltaX ), OnReachedDoor);
                        break;
                    }
                    
                }
                if (hit.collider.TryGetComponent(out Storage storage) && !storage.isSearched && storage.myRoom == myRoom)
                {
                    myStorage = storage;
                    StartMovement(storage.transform.position.x,OnReachedStorage);
                    break;
                }
                
                if (hit.collider.TryGetComponent(out Breakable br) && !br.isBroken && br.myRoom == myRoom)
                {
                    myBreakable = br;
                    StartMovement(br.transform.position.x,OnReachedBreakable);
                    break;
                }
            
                if (hit.collider.TryGetComponent(out Room room) && room == myRoom)
                {
                    Vector3 worldClickPos = new Vector3(clickPos.x, clickPos.y, 0);
                    if (room.IsPointInside(worldClickPos))
                    {
                        StartMovement(clickPos.x);
                        break;
                    }
                }
            }
        }
    }

    private void StartMovement(float targetX, Func<UniTask> onComplete = null)
    {
        var newCTS = new CancellationTokenSource();

        CancelCurrentMovement();
        movementCTS = newCTS;
        MoveToTargetAsync(targetX, onComplete, newCTS.Token).Forget();
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

    private async UniTask MoveToTargetAsync(float targetX, Func<UniTask> onComplete, CancellationToken ct)
    {
        int t = G.AudioManager.PlayLoop(R.Audio.step, 0.05f);
        try
        {
            bool b = transform.position.x - targetX > 0;
            playerAnim.SetFlip(b);
            playerAnim._targetRenderer.transform.localPosition = new Vector3(b ? 0.1f : 0, playerAnim._targetRenderer.transform.localPosition.y, playerAnim._targetRenderer.transform.localPosition.z);
            playerAnim.SetAnimation(walking);
            
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
                G.AudioManager.StopLoop(t);
                transform.position = new Vector3(
                    targetX, 
                    transform.position.y, 
                    transform.position.z
                );
                playerAnim.SetAnimation(standing);
                onComplete?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            G.AudioManager.StopLoop(t);
            // Движение было отменено - ничего не делаем
        }
    }

    private async UniTask OnReachedElevator()
    {
        float timeToGoUp = 0.4f;
        float deltaUp = 0.5f;
        
        
        if(await myElev.myLiftHandler.CallElevator(myElev) == false) return;
        _inAnim = true;
        //Go in lift
        playerAnim.SetAnimation(goUpLift);
        G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f);
        DOVirtual.DelayedCall(0.25f, () => { G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f); });
        await transform.GetChild(0).DOLocalMoveY(transform.GetChild(0).localPosition.y + deltaUp, timeToGoUp)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        playerAnim._targetRenderer.sortingOrder = 5;
        playerAnim.SetAnimation(standing);
        
        //CloseDoors and wait
        await myElev.myLiftHandler.TeleportPlayer(this, myElev);
        //GoOut
        playerAnim._targetRenderer.sortingOrder = 50;
        playerAnim.SetAnimation(goDownLift);
        G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f);
        DOVirtual.DelayedCall(0.25f, () => { G.AudioManager.PlayWithRandomPitch(R.Audio.step, 0.25f); });
        await transform.GetChild(0).DOLocalMoveY(0, timeToGoUp)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        
        playerAnim.SetAnimation(standing);
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
    private async UniTask OnReachedStorage()
    {
        _inAnim = true;
        
        playerAnim.SetAnimation(searching);
        G.AudioManager.PlaySound(R.Audio.looting);
        await UniTask.Delay(1400);
        
        myStorage.OpenAnim();
        myStorage.GetLoot();
        
        playerAnim.SetAnimation(standing);
        _inAnim = false;
    }
    private async UniTask OnReachedBreakable()
    {
        if (!G.Main.inventory.CheckWithItem(myBreakable.neededId))
        {
            myBreakable.ShowUI();
            return;
        }
        _inAnim = true;
        G.Main.inventory.RemoveCurrentItem();
        playerAnim.SetAnimation(searching);
        G.AudioManager.PlaySound(R.Audio.packost);
        await UniTask.Delay(1000);
        myBreakable.Break().Forget();

        playerAnim.SetAnimation(standing);
        _inAnim = false;
    }

    private void OnReachedPoint()
    {
        Debug.Log("Reached target point");
    }

    private void Update()
    {
        HandleInput();
    }
}
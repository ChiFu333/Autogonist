using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening; // Для анимации (нужен Asset DOTween)

public class PairLiftHandler : MonoBehaviour
{
    public bool isLockedByEnemy = false;
    [Header("Elevators")]
    [SerializeField] private Elevator elevatorA;
    [SerializeField] private Elevator elevatorB;
    
    private float travelTime = 2.8f; // Время "поездки" лифта
    
    private bool _isLiftMoving;
    
    private Vector3 _realLiftPos;
    private Vector3 _initialPositionA;
    private Vector3 _initialPositionB;
    
    private void Awake()
    {
        // Сохраняем начальные позиции (если лифт физически движется)
        _initialPositionA = elevatorA.transform.position;
        _initialPositionB = elevatorB.transform.position;

        _realLiftPos = _initialPositionA;
        
        elevatorA.FastLight(true);
        elevatorB.FastLight(false);
        
        elevatorA.myLiftHandler = this;
        elevatorB.myLiftHandler = this;
    }

    // Вызов лифта (например, при нажатии кнопки)
    public async UniTask<bool> CallElevator(Elevator fromElevator)
    {
        if (_isLiftMoving) return false;
        _isLiftMoving = true;

        if (_realLiftPos != ((fromElevator == elevatorA) ? _initialPositionA : _initialPositionB))
        {
            fromElevator.LightRed();
            Elevator otherElevator = (fromElevator == elevatorA) ? elevatorB : elevatorA;
            otherElevator.LightRed();
            //await otherElevator.CloseDoors();
            G.AudioManager.PlaySound(R.Audio.sfxlift);
            fromElevator.LightAll();
            DOTween.To(
                    () => _realLiftPos,
                    x => _realLiftPos = x,
                    (fromElevator == elevatorA) ? _initialPositionA : _initialPositionB,
                    travelTime
                )
                .SetEase(Ease.InOutSine)
                .OnComplete(() => _isLiftMoving = false);
            return false;
        }
        
        await fromElevator.OpenDoors();
        _isLiftMoving = false;
        return true;
    }

    // Телепортация игрока между лифтами
    public async UniTask TeleportPlayer(PlayerController player, Elevator fromElevator)
    {
        if (_isLiftMoving) return;
        
        Elevator targetElevator = (fromElevator == elevatorA) ? elevatorB : elevatorA;
        
        // Закрываем двери текущего лифта
        await fromElevator.CloseDoors();
        G.AudioManager.PlaySound(R.Audio.sfxlift);
        fromElevator.LightRed();
        targetElevator.LightAll();
        player.transform.position = targetElevator.ExitPoint.position;
        await DOTween.To(
            () => _realLiftPos,
            x => _realLiftPos = x,
            (fromElevator == elevatorA) ? _initialPositionB : _initialPositionA,
            travelTime
        ).SetEase(Ease.InOutSine)
        .AsyncWaitForCompletion();
        
        
        player.myRoom = targetElevator.myRoom;
        await targetElevator.OpenDoors();
        
        WaitAndCloseDoor(targetElevator).Forget();
    }
    public async UniTask TeleportPlayer(EnemyController enemy, Elevator fromElevator)
    {
        if (_isLiftMoving) return;
        
        Elevator targetElevator = (fromElevator == elevatorA) ? elevatorB : elevatorA;
        
        // Закрываем двери текущего лифта
        await fromElevator.CloseDoors();
        G.AudioManager.PlaySound(R.Audio.sfxlift);
        fromElevator.LightRed();
        targetElevator.LightAll();
        enemy.transform.position = targetElevator.ExitPoint.position;
        await DOTween.To(
                () => _realLiftPos,
                x => _realLiftPos = x,
                (fromElevator == elevatorA) ? _initialPositionB : _initialPositionA,
                travelTime
            ).SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
        
        
        enemy.myRoom = targetElevator.myRoom;
        await targetElevator.OpenDoors();
        
        WaitAndCloseDoor(targetElevator).Forget();
    }

    private async UniTask WaitAndCloseDoor(Elevator el)
    {
        await UniTask.Delay(500);
        el.CloseDoors().Forget();
    }
}
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class Elevator : MonoBehaviour
{
    [Header("References")]
    private Transform exitPoint; // Точка выхода из лифта

    private float timeToOpen = 0.5f;
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;// Аниматор дверей (опционально)
    [SerializeField] private List<SpriteRenderer> bulbs;
    public Transform ExitPoint => exitPoint;
    [HideInInspector] public Room myRoom;
    [HideInInspector] public PairLiftHandler myLiftHandler;

    private float localXPosLeftDoor, localXPosRightDoor;

    
    private void Awake()
    {
        myRoom = GetComponentInParent<Room>();
        exitPoint = transform.GetChild(0);

        localXPosLeftDoor = leftDoor.localPosition.x;
        localXPosRightDoor = rightDoor.localPosition.x;
    }
    
    // Анимация дверей (можно заменить на DOTween)
    public async UniTask OpenDoors()
    {
        float deltaX = 1;
        G.AudioManager.PlayWithRandomPitch(R.Audio.sfxliftopn, 0.1f);
        leftDoor.transform.DOLocalMoveX(localXPosLeftDoor - deltaX, timeToOpen);
        await rightDoor.transform.DOLocalMoveX(localXPosRightDoor + deltaX, timeToOpen)
            .AsyncWaitForCompletion();
        
    }

    public async UniTask CloseDoors()
    {
        G.AudioManager.PlayWithRandomPitch(R.Audio.sfxliftopn, 0.1f);
        leftDoor.transform.DOLocalMoveX(localXPosLeftDoor, timeToOpen);
        await rightDoor.transform.DOLocalMoveX(localXPosRightDoor, timeToOpen)
            .AsyncWaitForCompletion();
    }

    public async void LightAll()
    {
        foreach (var b in bulbs)
        {
            b.material.SetColor("_Color", Color.green);
            float currentGlow = 0;
            DOTween.To(
                () => currentGlow, // Геттер текущего значения
                x => {
                    currentGlow = x;
                    b.material.SetFloat("_Glow", x);
                },
                25,        // Конечное значение
                0.2f           // Длительность анимации
            ).SetEase(Ease.OutQuad);
            
            G.AudioManager.PlayWithRandomPitch(R.Audio.liftping1, 0.2f);
            await UniTask.Delay(800);
        }

        await UniTask.Delay(200);
        G.AudioManager.PlayWithRandomPitch(R.Audio.liftping, 0.1f);
    }

    public async void LightRed()
    {
        foreach (var b in bulbs)
        {
            b.material.SetColor("_Color", Color.red);
            b.material.SetFloat("_Glow", 15);
        }
    }

    public void FastLight(bool toGreen)
    {
        foreach (var b in bulbs)
        {
            if (toGreen)
            {
                b.material.SetColor("_Color", Color.green);
                b.material.SetFloat("_Glow", 25);
            }
            else
            {
                b.material.SetColor("_Color", Color.red);
                b.material.SetFloat("_Glow", 15);
            }
        }
    }
}


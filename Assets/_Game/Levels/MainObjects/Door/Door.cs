using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [Header("BetweenDoors")] 
    public Room left;
    public Room right;
    [Header("Anims")]
    [SerializeField] private AnimationDataSO openAnim;
    [SerializeField] private AnimationDataSO closeAnim;
    public float deltaX { get; private set; } = 1.3f;
    [Header("EndDoor")] 
    public bool isWorking = true;
    public UnityEvent goEvent;
    private AnimationController _animator;
    public SpriteRenderer render;
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        _animator = GetComponent<AnimationController>();
        _animator.Init();
    }

    public async UniTask OpenAnim()
    {
        G.AudioManager.PlayWithRandomPitch(R.Audio.sfxliftopn, 0.15f);
        await _animator.SetAnimOneShot(openAnim);
    }
    public async UniTask CloseAnim()
    {
        G.AudioManager.PlayWithRandomPitch(R.Audio.sfxliftopn, 0.15f);
        await _animator.SetAnimOneShot(closeAnim);
    }
}

using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class Breakable : MonoBehaviour
{
    public bool isBroken = false;
    public int neededId = -2;
    [HideInInspector] public Room myRoom;
    
    private Sequence _animationSequence;
    private bool _isShowing;

    [SerializeField] private GameObject hintObj;
    [SerializeField] private ParticleSystem ps;
    private float _showDuration = 0.2f;
    private float _hideDuration = 0.3f;
    
    private SpriteRenderer _renderer;
    private Tween _glowTween;
    
    void Start()
    {
        hintObj.transform.localScale = Vector3.zero;
        myRoom = GetComponentInParent<Room>();
        
        _renderer = GetComponent<SpriteRenderer>();
        
        _glowTween = DOTween.Sequence()
            .Append(_renderer.material.DOFloat(2.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .Append(_renderer.material.DOFloat(0.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .Append(_renderer.material.DOFloat(2.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart) // Бесконечное повторение
            .SetAutoKill(false);
    }

    public void ShowUI()
    {
        hintObj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = G.Main.itemsDatabase[neededId].selectedSprite;
        // Если анимация уже идет - убиваем ее
        if (_animationSequence != null && _animationSequence.IsActive())
        {
            _animationSequence.Kill();
        }

        G.AudioManager.PlayWithRandomPitch(R.Audio.request, 0.1f);
        _animationSequence = DOTween.Sequence();
        _animationSequence.Append(
            hintObj.transform.DOScale(Vector3.one, _showDuration)
                .SetEase(Ease.OutBack)
        );
        _animationSequence.AppendInterval(1);
        _animationSequence.Append(
            hintObj.transform.DOScale(Vector3.zero, _hideDuration)
                .SetEase(Ease.InBack)
        );
        
        _animationSequence.OnComplete(() => {
            _animationSequence = null;
            _isShowing = false;
        });
        
        _isShowing = true;
    }

    public async UniTaskVoid Break()
    {
        await BabaxAnim();
        
        ConveyorManager.Instance.ReduceAllConveyorsEfficiency();
        
        G.AudioManager.PlaySound(R.Audio.boom);
        G.Main.progressBar.UpdateProgressBar(G.Main.progressBar.currentValue - (100 * (1f/G.Main.itemsDatabase.Count)));
    }

    private async UniTask BabaxAnim()
    {
        float shakeInterval = 1f;
        float punchStrength = 0.15f;
        
        float colorChangeDuration = 7f;
        float punchDuration = 0.35f;
        
    
        _renderer.DOColor(Color.red, colorChangeDuration);
        
        // Первый панч
        G.AudioManager.PlayWithRandomPitch(R.Audio.littleBoom, 0.15f);
        await transform.DOPunchScale(Vector3.one * punchStrength, punchDuration, 10, 1f)
            .AsyncWaitForCompletion();
        await UniTask.WaitForSeconds(shakeInterval);
        
        // Второй панч
        G.AudioManager.PlayWithRandomPitch(R.Audio.littleBoom, 0.15f);
        await transform.DOPunchScale(Vector3.one * punchStrength, punchDuration, 10, 1f)
            .AsyncWaitForCompletion();
        await UniTask.WaitForSeconds(shakeInterval);
        
        LastBabax().Forget();
        
    }

    private async UniTask LastBabax()
    {
        float bigPunchStrength = 0.4f;
        float bigPunchDuration = 0.35f;
        transform.DOPunchScale(Vector3.one * bigPunchStrength, bigPunchDuration, 13, 1f);
        await UniTask.Delay(250);
        // Возвращаем цвет
        _renderer.DOKill();
        _renderer.color = new Color(150/256f, 60/256f, 57/256f);
        if (_glowTween != null && _glowTween.IsActive())
        {
            _glowTween.Kill();
            _glowTween = null;
            
            // Возвращаем значение в исходное состояние или 0, если нужно отключить свечение
            _renderer.material.SetFloat("_Glow", 0f);
        }
        ps.Play();
    }
}

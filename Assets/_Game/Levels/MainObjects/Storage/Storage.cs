using UnityEngine;
using DG.Tweening;
public class Storage : MonoBehaviour
{
    public bool isHereSomething = true;
    
    [SerializeField] public bool isSearched;
    [SerializeField] private Sprite openedSprite;
    [HideInInspector] public Room myRoom;

    private SpriteRenderer _renderer;
    private Tween _glowTween;
    void Start()
    {
        myRoom = GetComponentInParent<Room>();
        _renderer = GetComponent<SpriteRenderer>();
        
        _glowTween = DOTween.Sequence()
            .Append(_renderer.material.DOFloat(2.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .Append(_renderer.material.DOFloat(0.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .Append(_renderer.material.DOFloat(2.5f, "_Glow", 5).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart) // Бесконечное повторение
            .SetAutoKill(false);
    }

    public void OpenAnim()
    {
        //G.AudioManager.PlayWithRandomPitch(R.Audio.sfxliftopn, 0.15f); добавить звук открытия
        _renderer.sprite = openedSprite;
        isSearched = true;
        
        if (_glowTween != null && _glowTween.IsActive())
        {
            _glowTween.Kill();
            _glowTween = null;
            
            // Возвращаем значение в исходное состояние или 0, если нужно отключить свечение
            _renderer.material.SetFloat("_Glow", 0f);
        }
    }

    public void GetLoot()
    {
        if (isHereSomething)
        {
            G.AudioManager.PlaySound(R.Audio.pickup);
            G.Main.inventory.AddItemInInventory(G.Main.GetItem());
        }
    }
}

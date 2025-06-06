using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonScaling : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] public bool DoEnterAnim = true;
    [SerializeField] public UnityEvent OnClick;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!DoEnterAnim) return;
        
        G.AudioManager.PlaySound(R.Audio.MouseInButton);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 0.8f,0.1f).SetEase(Ease.OutBack))
            .Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 0.9f,0.2f).SetEase(Ease.OutBack));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!DoEnterAnim) return;
        
        GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one, 0.3f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        G.AudioManager.PlayWithRandomPitch(R.Audio.MouseClicked, 0.2f);
        Sequence mySequence = DOTween.Sequence();
        if (DoEnterAnim)
        {
            mySequence.Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 0.7f,0.1f))
                .Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 0.9f,0.1f));
        }
        else
        {
            mySequence.Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 0.8f,0.1f))
                .Append(GetComponent<RectTransform>().GetChild(1).DOScale(Vector3.one * 1f,0.1f));
        }
        OnClick.Invoke();
    }

    public void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Serialization;

public class SliderProgressBar : MonoBehaviour
{
    [FormerlySerializedAs("startValue")] [Header("Settings")] 
    public float currentValue;
    [SerializeField] private Color minColor = Color.green;
    [SerializeField] private Color gray;

    private Color currentColor;
    
    [Header("References")]
    [SerializeField] private List<Image> images;

    private void Awake()
    {
    }

    public void Start()
    {
        UpdateProgressBar(currentValue);
    }

    public async void UpdateProgressBar(float value)
    {
        float intensity = 3f;
        if (value <= 5) value = 0;
        currentValue = value;
        // Нормализуем значение от 0 до 1
        float normalizedValue = Mathf.Clamp01(value / 100f);
        currentColor = minColor;

        int bulbToLight = (int)(normalizedValue * images.Count);
        for (int i = 0; i < images.Count; i++)
        {
            Color c = (i < bulbToLight) ? currentColor : gray;
            
            Color brighterColor = new Color(
                c.r * intensity,
                c.g * intensity,
                c.b * intensity,
                c.a // Альфа-канал оставляем без изменений
            );
            var _sequence = DOTween.Sequence();
            _sequence.Append(images[i].DOColor(brighterColor, 0.1f));
            _sequence.Append(images[i].DOColor(c, 0.05f));
            await UniTask.Delay(5);
        }
        if(value == 0)
            G.Main.DoBlockout();
    }
}
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class ConveyorManager : MonoBehaviour
{
    public Tilemap tilemap1, tilemap2;
    public static ConveyorManager Instance { get; private set; }

    public List<ConveyorBelt> _allConveyors = new List<ConveyorBelt>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    public void RegisterConveyor(ConveyorBelt conveyor)
    {
        if (!_allConveyors.Contains(conveyor))
        {
            _allConveyors.Add(conveyor);
        }
    }

    public void UnregisterConveyor(ConveyorBelt conveyor)
    {
        if (_allConveyors.Contains(conveyor))
        {
            _allConveyors.Remove(conveyor);
        }
    }

    // Понижение эффективности всех конвейеров
    public void ReduceAllConveyorsEfficiency()
    {
        foreach (var conveyor in _allConveyors)
        {
            conveyor.SetReducedEfficiency();
        }
    }

    // Полная остановка всех конвейеров
    public void StopAllConveyors()
    {
        foreach (var conveyor in _allConveyors)
        {
            conveyor.StopConveyor();
        }

        DisableAnimations(tilemap1);
        DisableAnimations(tilemap2);

    }

    private void DisableAnimations(Tilemap tilemap)
    {
            tilemap.animationFrameRate = 0;
    }   
}
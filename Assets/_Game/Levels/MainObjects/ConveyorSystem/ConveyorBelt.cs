using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Serialization;

public class ConveyorBelt : MonoBehaviour
{
    public enum Direction { Left, Right }

    [Header("Settings")]
    [SerializeField] private Direction _direction = Direction.Right;
    [SerializeField] private float _baseSpawnInterval = 2f;
    [SerializeField] private float _baseSpeed = 1f;
    [SerializeField] private float _destroyXPosition = 10f;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private int _initialItemsCount = 5; // Количество объектов при старте

    private List<GameObject> _activeItems = new List<GameObject>();
    private float _timer;
    private float _currentSpeed;
    private float _originalSpeed;
    private float _itemWidth; // Ширина объекта для правильного размещения
    
    private float _currentSpawnInterval;
    private bool _isActive = true;
    private float _efficiencyMultiplier = 1f;

    private void Awake()
    {
        _currentSpeed = _baseSpeed;
        _currentSpawnInterval = _baseSpawnInterval;
        
        // Регистрируемся в менеджере
       
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении
        ConveyorManager.Instance?.UnregisterConveyor(this);
    }

    // Установка пониженной эффективности
    public void SetReducedEfficiency()
    {
        _efficiencyMultiplier *= 0.5f;
        UpdateWorkingParameters();
    }

    // Остановка конкретного конвейера
    public void StopConveyor()
    {
        _isActive = false;
        UpdateWorkingParameters();
    }

    // Обновление рабочих параметров
    private void UpdateWorkingParameters()
    {
        if (_isActive)
        {
            _currentSpeed = _baseSpeed * _efficiencyMultiplier;
            _currentSpawnInterval = _baseSpawnInterval / _efficiencyMultiplier;
        }
        else
        {
            _currentSpeed = 0f;
        }
    }

    private void Start()
    {
        // Создаем начальные объекты
        ConveyorManager.Instance?.RegisterConveyor(this);
        InitializeConveyor();
    }

    private void InitializeConveyor()
    {
        float directionMultiplier = (_direction == Direction.Right) ? 1f : -1f;
        float spacing = _baseSpeed * _currentSpawnInterval; // Расстояние между объектами
        
        for (int i = 0; i < _initialItemsCount; i++)
        {
            // Позиция с учетом направления и расстояния между объектами
            Vector3 spawnPos = transform.position + 
                             Vector3.right * directionMultiplier * 
                             (i * spacing + i * _itemWidth);
            
            GameObject newItem = Instantiate(_itemPrefab, spawnPos, Quaternion.identity);
            newItem.transform.parent = transform;
            _activeItems.Add(newItem);
        }
    }

    private void Update()
    {
        // Спавн новых объектов по таймеру
        _timer += Time.deltaTime;
        if (_timer >= _currentSpawnInterval)
        {
            SpawnItem();
            _timer = 0f;
        }

        // Движение объектов
        MoveItems();
    }

    private void SpawnItem()
    {
        if (_itemPrefab == null) return;

        GameObject newItem = Instantiate(_itemPrefab, transform.position, Quaternion.identity);
        newItem.transform.parent = transform;
        _activeItems.Add(newItem);
    }

    private void MoveItems()
    {
        float directionMultiplier = (_direction == Direction.Right) ? 1f : -1f;
        float speed = _currentSpeed;

        for (int i = _activeItems.Count - 1; i >= 0; i--)
        {
            if (_activeItems[i] == null)
            {
                _activeItems.RemoveAt(i);
                continue;
            }

            // Перемещение объекта
            _activeItems[i].transform.Translate(
                Vector3.right * (directionMultiplier * speed * Time.deltaTime), 
                Space.World);

            // Проверка на достижение границы
            float xPos = _activeItems[i].transform.localPosition.x;
            if ((_direction == Direction.Right && xPos >= _destroyXPosition) ||
                (_direction == Direction.Left && xPos <= -_destroyXPosition))
            {
                DestroyItem(_activeItems[i]);
                _activeItems.RemoveAt(i);
            }
        }
    }

    private void DestroyItem(GameObject item)
    {
        if (item == null) return;
        Destroy(item);
    }
}
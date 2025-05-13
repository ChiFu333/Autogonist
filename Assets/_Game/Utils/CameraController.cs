using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Pan Settings")]
    [SerializeField] private Vector2 panLimitX = new Vector2(-10f, 10f);
    [SerializeField] private Vector2 panLimitY = new Vector2(-5f, 5f);

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isPanning = false;

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            
            // После изменения зума корректируем позицию камеры, чтобы оставаться в пределах
            ClampCameraPosition();
        }
    }

    private void HandlePan()
    {
        // Начало перемещения
        if (Input.GetMouseButtonDown(2)) // Средняя кнопка мыши
        {
            isPanning = true;
            lastMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // Окончание перемещения
        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        // Перемещение камеры
        if (isPanning)
        {
            Vector3 currentMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = lastMousePosition - currentMousePosition;
            transform.position += delta;
            
            ClampCameraPosition();
            
            lastMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void ClampCameraPosition()
    {
        // Рассчитываем видимую область камеры
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        
        // Вычисляем допустимые границы с учетом текущего зума
        float minX = panLimitX.x + cameraWidth;
        float maxX = panLimitX.y - cameraWidth;
        float minY = panLimitY.x + cameraHeight;
        float maxY = panLimitY.y - cameraHeight;
        
        // Ограничиваем позицию камеры
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
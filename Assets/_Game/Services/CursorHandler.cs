using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class CursorHandler : MonoBehaviour, IService
{
    [Header("Main Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 hotSpot = Vector2.zero;
    private float trailWidth = 0.2f;
    private float trailTime = 0.4f;
    private Color trailColor = Color.white;
    private float size = 0.08f;
    
    private Canvas uiCanvas;       // Для основного курсора (Screen Space - Overlay)
    private Canvas trailCanvas;   // Для шлейфа (World Space)
    private Image cursorImage;
    private RectTransform cursorTransform;
    private TrailRenderer trailRenderer;
    private Camera overlayCamera;

    public void Start()
    {
        Init(true);
    }

    public void Init(bool showWhenInit)
    {
        Cursor.visible = false;
        CreateCanvases();
        CreateCursorObject();
        CreateTrailObject();
    }

    private void CreateCanvases()
    {
        // Основной UI Canvas (для курсора)
        GameObject uiCanvasObj = new GameObject("UICursorCanvas");
        uiCanvas = uiCanvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 32767;
        DontDestroyOnLoad(uiCanvasObj);

        // Canvas для шлейфа (World Space)
        GameObject trailCanvasObj = new GameObject("TrailCanvas");
        trailCanvas = trailCanvasObj.AddComponent<Canvas>();
        trailCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        trailCanvas.worldCamera = Camera.main;
        trailCanvas.planeDistance = 1; // Ближе к камере, чем основной UI
        trailCanvas.sortingOrder = 32766; // Чуть ниже основного курсора
        DontDestroyOnLoad(trailCanvasObj);
    }

    private void CreateCursorObject()
    {
        GameObject cursorObj = new GameObject("UICursor");
        cursorObj.transform.SetParent(uiCanvas.transform, false);
        cursorObj.transform.localScale = Vector3.one * size;
        cursorImage = cursorObj.AddComponent<Image>();
        cursorImage.sprite = Sprite.Create(
            cursorTexture,
            new Rect(0, 0, cursorTexture.width, cursorTexture.height),
            hotSpot
        );
        cursorImage.raycastTarget = false;

        cursorTransform = cursorObj.GetComponent<RectTransform>();
        cursorTransform.sizeDelta = new Vector2(cursorTexture.width, cursorTexture.height);
    }

    private void CreateTrailObject()
    {
        GameObject trailObj = new GameObject("CursorTrail");
        trailObj.transform.SetParent(trailCanvas.transform, false);

        // Добавляем пустой Graphic (требуется для TrailRenderer в UI)
        var graphic = trailObj.AddComponent<Image>();
        graphic.color = Color.clear;
        graphic.raycastTarget = false;

        // Настраиваем TrailRenderer
        trailRenderer = trailObj.AddComponent<TrailRenderer>();
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = 0;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.colorGradient = CreateGradient();

        // Связываем позицию с курсором
        trailObj.AddComponent<TrailPositionUpdater>().Init(cursorTransform);
    }

    private Gradient CreateGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(trailColor, 0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        return gradient;
    }

    private void Update()
    {
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        if (cursorTransform == null) return;

        Vector2 cursorPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.transform as RectTransform,
            Input.mousePosition,
            null,
            out cursorPos
        );
        cursorTransform.localPosition = cursorPos;
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }
}

// Вспомогательный класс для обновления позиции TrailRenderer
public class TrailPositionUpdater : MonoBehaviour
{
    private RectTransform target;
    private Camera mainCamera;

    public void Init(RectTransform target)
    {
        this.target = target;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (target == null) return;
        
        // Конвертируем UI-позицию в мировые координаты
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            RectTransformUtility.WorldToScreenPoint(null, target.position)
        );
        transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    }
}
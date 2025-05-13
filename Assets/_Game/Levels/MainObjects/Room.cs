using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Settings")]
    //[SerializeField] private float leftBoundary = -5f;
    //[SerializeField] private float rightBoundary = 5f;
    [SerializeField] private Collider2D roomCollider; // Опционально, если форма сложная

   //public float LeftBoundary => leftBoundary;
    //public float RightBoundary => rightBoundary;

    // Проверяет, находится ли точка внутри комнаты (по X или с учётом Collider2D)
    public bool IsPointInside(Vector3 point)
    {
        if (roomCollider != null)
            return roomCollider.OverlapPoint(point);
        return false;
        //return point.x >= leftBoundary && point.x <= rightBoundary;
    }

    // Визуализация границ в редакторе
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.green;
    //     Vector3 leftPos = new Vector3(leftBoundary, transform.position.y, 0);
    //     Vector3 rightPos = new Vector3(rightBoundary, transform.position.y, 0);
    //     Gizmos.DrawLine(leftPos + Vector3.up * 2, leftPos + Vector3.down * 2);
    //     Gizmos.DrawLine(rightPos + Vector3.up * 2, rightPos + Vector3.down * 2);
    //     Gizmos.DrawLine(leftPos, rightPos);
    // }
}
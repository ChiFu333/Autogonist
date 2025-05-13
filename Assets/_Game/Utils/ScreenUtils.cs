using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class ScreenUtils {
    public static Camera cam = null;
    public static Vector2 WorldMouse(Camera cam = null) {
        if (cam == null) return GetCamera().ScreenToWorldPoint(Input.mousePosition);
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public static Vector3 World3DMouse() {
        float d;
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = GetCamera().ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out d)) {
            return ray.GetPoint(d);
        }
        return Vector3.zero;
    }
    
    public static Vector2 WorldPosition(Vector2 position) {
        return GetCamera().ScreenToWorldPoint(position);
    }

    public static Vector2 ScreenPosition(Vector3 position) {
        return GetCamera().WorldToScreenPoint(position);
    }

    public static bool IsPositionOnScreen(Vector2 position) {
        Vector2 ScreenPosition = GetCamera().WorldToScreenPoint(position);
        return ScreenPosition.x > 0 && ScreenPosition.x < cam.pixelWidth &&
               ScreenPosition.y > 0 && ScreenPosition.y < cam.pixelHeight;
    }

    public static bool IsAABBOnScreen(AABB aabb) {
        if (IsPositionOnScreen(aabb.min)) return true;
        if (IsPositionOnScreen(aabb.max)) return true;
        if (IsPositionOnScreen(new Vector2(aabb.min.x, aabb.max.y))) return true;
        if (IsPositionOnScreen(new Vector2(aabb.max.x, aabb.min.y))) return true;
        return false;
    }

    public static bool IsMouseOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        int count = raycastResults.Count;
        //There goes implementation of ignorable interface detection
        return count > 0;
    }

    public static List<GameObject> GetObjectsUnderMouse() {
        Ray ray = GetCamera().ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin,ray.direction);
        return hits.Select(i => i.collider.gameObject).ToList();
    }

    public static Camera GetCamera() {
        if (cam == null) { CacheCamera(Camera.main); }
        return cam;
    }

    private static void CacheCamera(Camera newCamera) {
        cam = newCamera;
    }
}
public struct AABB {
    public AABB(Vector2 min, Vector2 max) {
        this.min = min; this.max = max;
    }
    public Vector2 min, max;
    public Vector2 GetCenter() {
        return (max + min) / 2.0f;
    }
    public Vector2 GetExtents() {
        return (max - min) / 2.0f;
    }
    public static AABB operator +(AABB aabb, Vector2 position) {
        return new AABB(aabb.min + position, aabb.max + position);
    }
    public void DrawGizmo() {
        Gizmos.DrawLine(min, new Vector3(min.x, max.y));
        Gizmos.DrawLine(new Vector3(min.x, max.y), max);
        Gizmos.DrawLine(max, new Vector3(max.x, min.y));
        Gizmos.DrawLine(new Vector3(max.x, min.y), min);
    }
}
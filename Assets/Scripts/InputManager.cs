using ECS;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    
    [SerializeField]
    private Texture2D[] crossHairs;

    private void Update()
    {
        if (!_secondaryDown && Input.GetKeyDown(KeyCode.Space))
        {
            _secondaryDown = true;
        }

        if (!_actionDown && Input.GetKey(KeyCode.E))
        {
            _actionDown = true;
        }

        _mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    public void SetCrossHair(int index)
    {
        Cursor.SetCursor(crossHairs[index], Vector2.one * 0.5f, CursorMode.Auto);
    }

    public void RemoveCrossHair()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
    public Vector3 SnipePosition()
    {
        var pos = (cam.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0.5f)) * 2f;
        
        pos.x = Mathf.Clamp(pos.x, -1, 1);
        pos.y = Mathf.Clamp(pos.y, -1, 1);
        pos.z = 0;
    
        return pos;
    }
    
    public float GetAngle(Transform relativeTo)
    {
        Vector2 direction = _mousePosition - relativeTo.position;

        return Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;
    }

    private float _angle;

    private bool _secondaryDown;
    private bool _actionDown;

    private Vector3 _mousePosition;

    public Intent BuildIntent(Transform relativeTo)
    {
        var xAxis = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        var yAxis = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        var angle = GetAngle(relativeTo);

        var intent = new Intent()
        {
            _movement = Vector2.ClampMagnitude(new Vector2(xAxis, yAxis), 1),
            _primary = Input.GetMouseButton(0),
            _secondary = _secondaryDown,
            _action = _actionDown,
            _angle = angle
        };

        _secondaryDown = false;
        _actionDown = false;

        return intent;
    }
}

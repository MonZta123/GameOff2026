using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class MarkerManager : MonoBehaviour
{
    [SerializeField]
    private Camera camera;

    public static List<Marker> Markers { get; set; } = new();

    [SerializeField]
    private MarkerArrow arrowPrefab;

    private readonly MarkerArrow[] _arrowList = new MarkerArrow[6];

    private void Awake()
    {
        for (var i = 0; i < 6; i++)
            _arrowList[i] = Instantiate(arrowPrefab, transform);
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
        if (!visible)
        {
            foreach (var arrow in _arrowList)
                arrow.gameObject.SetActive(false);
        }
    }

    private bool _visible = true;

    private void LateUpdate()
    {
        if (!_visible)
            return;

        var used = 0;

        for (var i = 0; i < Markers.Count && used < _arrowList.Length; i++)
        {
            var marker = Markers[i];

            if (!marker.UsesArrow)
                continue;

            var position = camera.WorldToViewportPoint(marker.transform.position);

            if (position.x is >= 0 and <= 1 && position.y is >= 0 and <= 1)
                continue;

            var arrow = _arrowList[used];

            arrow.gameObject.SetActive(true);

            var viewPortVec = new Vector2(position.x, position.y);

            var newPointX = Mathf.Clamp(position.x, 0f, 1f);
            var newPointY = Mathf.Clamp(position.y, 0f, 1f);

            var newPos = new Vector2(newPointX, newPointY);

            var newTextX = Mathf.Clamp(position.x, 0.2f, 0.8f);
            var newTextY = Mathf.Clamp(position.y, 0.2f, 0.8f);

            var newTextPos = new Vector2(newTextX, newTextY);
            var arrowPositionX = Mathf.Clamp(position.x, 0.15f, 0.85f);
            var arrowPositionY = Mathf.Clamp(position.y, 0.15f, 0.85f);

            var arrowPosition = new Vector2(arrowPositionX, arrowPositionY);

            var vec = viewPortVec - arrowPosition;
            vec.x *= camera.aspect;

            arrow.SetVector(vec);
            arrow.SetText(marker.Text);
            arrow.SetPosition(camera.ViewportToWorldPoint(arrowPosition));
            arrow.SetTextPosition(camera.ViewportToWorldPoint(newTextPos));

            used++;
        }

        for (var i = used; i < _arrowList.Length; i++)
        {
            _arrowList[i].gameObject.SetActive(false);
        }
    }
}

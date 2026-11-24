using System;
using UnityEngine;

public class CutsceneBouncer : MonoBehaviour
{
    [SerializeField]
    private float bounceSpeed = 2f;

    [SerializeField]
    private float bounceHeight = 0.2f;

    [SerializeField]
    private float bounceAngle = 11.25f;

    private float _count;

    private void OnEnable()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        _count = 0;
        _previousPosition = null;
    }

    private void LateUpdate()
    {
        if (_previousPosition != null)
        {
            var distance = Vector2.Distance(transform.parent.position, _previousPosition.Value);

            var deltaDistance = (distance / Time.deltaTime);

            if (distance > 1f)
            {
                _previousPosition = transform.parent.position;
                return;
            }

            if (deltaDistance < 0.01f)
            {
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
                _count = 0;
            }
            else
            {
                _count += distance * bounceSpeed;
                if (_count >= 2 * Mathf.PI)
                    _count -= 2 * Mathf.PI;

                transform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(_count)) * bounceHeight, 0);
                transform.localRotation = Quaternion.Euler(0, 0, (Mathf.Sin(_count + Mathf.PI / 2f) * bounceAngle));
            }
        }

        _previousPosition = transform.parent.position;
    }

    private Vector2? _previousPosition;
}

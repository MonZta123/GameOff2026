using UnityEngine;

public class PositionSmoother : MonoBehaviour
{
    [SerializeField]
    private float positionSmoothSpeed = 15f;

    [SerializeField]
    private float teleportThreshold = 5f;

    public bool IsSmoothingPosition { get; private set; }

    private Transform _parentTransform;
    private Vector3 _lastParentPosition;
    private Vector3 _visualPositionOffset;

    public void SetSmoothPosition(bool value)
    {
        IsSmoothingPosition = value;

        _visualPositionOffset = Vector3.zero;
        transform.localPosition = Vector3.zero;
        _lastParentPosition = _parentTransform.position;
    }

    private void Awake()
    {
        _parentTransform = transform.parent;
    }

    private void Update()
    {
        if (!IsSmoothingPosition)
            return;

        var parentMovement = _parentTransform.position - _lastParentPosition;
        if (parentMovement.magnitude > teleportThreshold)
        {
            _visualPositionOffset = Vector3.zero;
        }
        else
        {
            _visualPositionOffset -= parentMovement;
        }
        
        var t = 1f - Mathf.Exp(-positionSmoothSpeed * Time.deltaTime);

        _visualPositionOffset = Vector3.Lerp(_visualPositionOffset, Vector3.zero, t);
        transform.localPosition = _parentTransform.InverseTransformDirection(_visualPositionOffset);
        _lastParentPosition = _parentTransform.position;
    }
}

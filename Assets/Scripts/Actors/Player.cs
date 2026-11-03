using UnityEditor;
using UnityEngine;

namespace Actors
{
    public class Player : MonoBehaviour, IPoolable<ECS.Entities.Player>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;

        [SerializeField]
        private SpriteRenderer characterRenderer;

        [SerializeField]
        private SpriteRenderer gunRenderer;

        [SerializeField]
        private Sprite[] characterSprites;
        
        public short Id { get; private set; }

        private bool _activePlayer;

        public void Init(ECS.Entities.Player entity)
        {
            Id = entity._id;
            transform.position = entity._position;
            characterRenderer.sprite = characterSprites[(int)entity._type];
        }

        public void SetActivePlayer(bool value)
        {
            _activePlayer = value;
            if (!positionSmoother.IsSmoothingPosition)
                positionSmoother.SetSmoothPosition(true);
        }

        public void SetAngle(float angle)
        {
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void SetState(ECS.Entities.Player entity)
        {
            transform.position = entity._position;

            if (!_activePlayer)
                gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);
        }

        public void Release()
        {
            Id = 0;
            positionSmoother.SetSmoothPosition(false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 2.5f, Id.ToString());
        }
#endif
    }
}

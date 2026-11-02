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
        
        public short Id { get; private set; }

        public void Init(ECS.Entities.Player entity)
        {
            Id = entity._id;
            positionSmoother.SetSmoothPosition(true);
        }

        public void SetState(ECS.Entities.Player entity)
        {
            transform.position = entity.position;
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

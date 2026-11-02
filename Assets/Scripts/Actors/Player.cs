using UnityEditor;
using UnityEngine;

namespace Actors
{
    public class Player : MonoBehaviour, IPoolable<ECS.Entities.Player>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;
        
        public short Id { get; private set; }

        public void Init(ECS.Entities.Player entity)
        {
            Id = entity._id;
            positionSmoother.SetSmoothPosition(true);
        }

        public void SetState(ECS.Entities.Player entity)
        {
            transform.position = new Vector3(entity.position.x, 0, entity.position.y);
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

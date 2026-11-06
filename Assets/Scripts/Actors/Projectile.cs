using UnityEngine;

namespace Actors
{
    public class Projectile : MonoBehaviour, IEntityView<ECS.Entities.Projectile>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;

        public void Init(ECS.Entities.Projectile entity)
        {
        }

        public void SetState(ECS.Entities.Projectile entity)
        {
            transform.position = entity._position;

            if (!positionSmoother.IsSmoothingPosition)
                positionSmoother.SetSmoothPosition(true);
        }

        public void Release()
        {
            positionSmoother.SetSmoothPosition(false);
        }
    }
}

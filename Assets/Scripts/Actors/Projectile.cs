using UnityEngine;

namespace Actors
{
    public class Projectile : MonoBehaviour, IPoolable<ECS.Entities.Projectile>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;

        public short Id { get; private set; }

        public void Init(ECS.Entities.Projectile entity)
        {
            Id = entity._id;
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
            Id = 0;
        }
    }
}

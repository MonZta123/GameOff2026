using UnityEngine;

namespace Actors
{
    public class Projectile : MonoBehaviour, IPoolable<ECS.Entities.Projectile>
    {
        public short Id { get; private set; }
        
        public void Init(ECS.Entities.Projectile entity)
        {
            Id = entity._id;
        }

        public void SetState(ECS.Entities.Projectile entity)
        {
            transform.position = entity._position;
        }
        
        public void Release()
        {
            Id = 0;
        }
    }
}

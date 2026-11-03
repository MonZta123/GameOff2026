using UnityEngine;

namespace Actors
{
    public class Enemy : MonoBehaviour, IPoolable<ECS.Entities.Enemy>
    {
        public short Id { get; private set; }
        
        public void Init(ECS.Entities.Enemy entity)
        {
            Id = entity.id;
        }

        public void SetState(ECS.Entities.Enemy entity)
        {
            
        }
        
        public void Release()
        {
            Id = 0;
        }
    }
}

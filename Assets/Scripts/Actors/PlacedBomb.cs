using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Actors
{
    public class PlacedBomb : MonoBehaviour, IEntityView<ECS.Entities.PlacedBomb>
    {
        public void Init(ECS.Entities.PlacedBomb entity)
        {
            transform.position = entity._position + Random.insideUnitCircle;
            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        public void SetState(ECS.Entities.PlacedBomb entity, int tick)
        {
         
        }

        public void Release()
        {
            transform.position = Vector2.zero;
            transform.rotation = quaternion.identity;
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Actors
{
    public class Player :  MonoBehaviour, IPoolable<ECS.Entities.Player>
    {
        public short Id { get; private set; }
        
        public void Init(ECS.Entities.Player entity)
        {
            Id = entity._id;
        }

        public void SetState(ECS.Entities.Player entity)
        {
            
        }
        
        public void Release()
        {
            Id = 0;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 2.5f, Id.ToString());
        }
#endif
    }
}

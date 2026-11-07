using UnityEditor;
using UnityEngine;

namespace Actors
{
    public class Enemy : MonoBehaviour, IEntityView<ECS.Entities.Enemy>
    {
        [SerializeField]
        private SpriteRenderer characterRenderer;

        [SerializeField]
        private SpriteRenderer gunRenderer;

        public void Init(ECS.Entities.Enemy entity)
        {
        }

        public void SetState(ECS.Entities.Enemy entity, int tick)
        {
            transform.position = entity._position;
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);

            _health = entity._health;
        }

        private float _health;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position, "" + _health);
        }
#endif

        public void Release()
        {
        }
    }
}

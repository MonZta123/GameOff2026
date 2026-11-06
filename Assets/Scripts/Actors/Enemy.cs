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

        public void SetState(ECS.Entities.Enemy entity)
        {
            transform.position = entity._position;
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);

            if (!Mathf.Approximately(_health, entity._health))
            {
                Debug.Log(entity._health);
            }

            _health = entity._health;
        }

        private float _health;

        public void Release()
        {
            Debug.Log("Dead 💀"); 
        }
    }
}

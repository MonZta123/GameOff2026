using UnityEngine;

namespace Actors
{
    public class Enemy : MonoBehaviour, IPoolable<ECS.Entities.Enemy>
    {
        [SerializeField]
        private SpriteRenderer characterRenderer;

        [SerializeField]
        private SpriteRenderer gunRenderer;

        public short Id { get; private set; }

        public void Init(ECS.Entities.Enemy entity)
        {
            Id = entity._id;
        }

        public void SetState(ECS.Entities.Enemy entity)
        {
            transform.position = entity._position;

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
            Id = 0;
        }
    }
}

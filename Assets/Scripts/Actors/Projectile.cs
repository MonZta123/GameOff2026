using UnityEngine;

namespace Actors
{
    public class Projectile : MonoBehaviour, IEntityView<ECS.Entities.Projectile>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Sprite[] sprites;

        public void Init(ECS.Entities.Projectile entity)
        {
            transform.position = entity._position;
            transform.rotation = Quaternion.Euler(0f, 0f, entity._angle + 180f);
            spriteRenderer.sprite = sprites[(int)entity._type];
        }

        public void SetState(ECS.Entities.Projectile entity, int tick)
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

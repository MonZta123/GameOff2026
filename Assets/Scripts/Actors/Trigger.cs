using UnityEngine;
using UnityEngine.Tilemaps;

namespace Actors
{
    public class Trigger : MonoBehaviour, IEntityView<ECS.Entities.Trigger>
    {
        [SerializeField]
        private Tilemap tilemap;

        [SerializeField]
        private TileBase hackerZone;
        
        [SerializeField]
        private TileBase demolitionZone;
        
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Sprite mine;

        public void Init(ECS.Entities.Trigger entity)
        {
            if (entity._triggerType is ECS.Entities.TriggerType.HackerZone or ECS.Entities.TriggerType.DemolitionZone)
            {
                var sizeX = entity._rightTop.x - entity._leftBottom.x;
                var sizeY = entity._rightTop.y - entity._leftBottom.y;

                for (var i = 0; i < sizeX; i++)
                for (var j = 0; j < sizeY; j++)
                    tilemap.SetTile(new Vector3Int((int)entity._leftBottom.x + i, (int)entity._leftBottom.y + j, 0),
                        demolitionZone);
            }
            
            else if (entity._triggerType == ECS.Entities.TriggerType.Mine)
            {
                spriteRenderer.sprite = mine;
                transform.position = (entity._leftBottom + entity._rightTop) / 2f;
            }
        }

        public void SetState(ECS.Entities.Trigger entity, int tick)
        {
        }

        public void Release()
        {
        }
    }
}

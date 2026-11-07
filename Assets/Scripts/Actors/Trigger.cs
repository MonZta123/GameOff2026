using UnityEngine;
using UnityEngine.Tilemaps;

namespace Actors
{
    public class Trigger : MonoBehaviour, IEntityView<ECS.Entities.Trigger>
    {
        [SerializeField]
        private Tilemap _tilemap;

        [SerializeField]
        private TileBase _tilebase;
        
        public void Init(ECS.Entities.Trigger entity)
        {
            // transform.position = (entity._leftBottom + entity._rightTop) / 2f;
            
            var sizeX = entity._rightTop.x - entity._leftBottom.x;
            var sizeY = entity._rightTop.y - entity._leftBottom.y;
            
            for (var i = 0; i < sizeX; i++)
                for (var j = 0; j < sizeY; j++)
                    _tilemap.SetTile(new Vector3Int((int)entity._leftBottom.x + i, (int)entity._leftBottom.y + j, 0), _tilebase);
        }

        public void SetState(ECS.Entities.Trigger entity)
        {
            
        }

        public void Release()
        {
            
        }
    }
}

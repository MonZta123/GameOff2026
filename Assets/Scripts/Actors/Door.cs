using ECS;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Actors
{
    public class Door : MonoBehaviour, IEntityView<ECS.Entities.Door>
    {
        [SerializeField]
        private TileBase verticalSprite;

        [SerializeField]
        private TileBase horizontalSprite;

        [SerializeField]
        private Tilemap doorRenderer;

        public short Id { get; private set; }

        private bool _activePlayer;

        public void Init(ECS.Entities.Door entity)
        {
            transform.position = entity._leftBottom;

            if (entity._direction == Direction.Down)
            {
                for (var i = 0; i < entity._rightTop.y - entity._leftBottom.y; i++)
                {
                    doorRenderer.SetTile(new Vector3Int(0, i, 0), verticalSprite);
                }
            }

            if (entity._direction == Direction.Right)
            {
                for (var i = 0; i < entity._rightTop.x - entity._leftBottom.x; i++)
                {
                    doorRenderer.SetTile(new Vector3Int(i, 0, 0), horizontalSprite);
                }
            }
        }

        public void SetState(ECS.Entities.Door entity, int tick)
        {
        }

        public void Release()
        {
            doorRenderer.ClearAllTiles();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 2.5f, Id.ToString());
        }
#endif
    }
}

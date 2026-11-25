using ECS;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Actors
{
    public enum ShowMarkerFor
    {
        None,
        Heavy,
        Demolition
    }

    public class Door : MonoBehaviour, IEntityView<ECS.Entities.Door>
    {
        [SerializeField]
        private TileBase verticalSprite;

        [SerializeField]
        private TileBase horizontalSprite;

        [SerializeField]
        private Tilemap doorRenderer;

        [SerializeField]
        private Marker marker;

        public static ShowMarkerFor ShowMarkersFor = ShowMarkerFor.None;

        public short Id { get; private set; }

        private bool _activePlayer;

        private ShowMarkerFor _appliedState;

        public void Init(ECS.Entities.Door entity)
        {
            marker.gameObject.SetActive(false);
            _appliedState = ShowMarkerFor.None;
            
            SetBubble();

            var markerPos = new Vector2((entity._rightTop.x - entity._leftBottom.x) / 2f,
                entity._rightTop.y - entity._leftBottom.y + 1.0f);

            marker.transform.localPosition = markerPos;

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

        private void SetBubble()
        {
            if (ShowMarkersFor == ShowMarkerFor.None && _appliedState != ShowMarkerFor.None)
            {
                marker.gameObject.SetActive(false);
                _appliedState = ShowMarkerFor.None;
            }

            if (ShowMarkersFor == ShowMarkerFor.Demolition && _appliedState != ShowMarkerFor.Demolition)
            {
                marker.SetText(string.Empty, "Trien can destroy doors with leftclick!");
                marker.gameObject.SetActive(true);
                _appliedState = ShowMarkerFor.Demolition;
            }

            if (ShowMarkersFor == ShowMarkerFor.Heavy && _appliedState != ShowMarkerFor.Heavy)
            {
                marker.SetText(string.Empty, "Olga can smash through with spacebar!");
                marker.gameObject.SetActive(true);
                _appliedState = ShowMarkerFor.Heavy;
            }
        }

        public void SetState(ECS.Entities.Door entity, int tick)
        {
            SetBubble();
        }

        public void Release()
        {
            doorRenderer.ClearAllTiles();
            marker.gameObject.SetActive(false);
            _appliedState = ShowMarkerFor.None;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up * 2.5f, Id.ToString());
        }
#endif
    }
}

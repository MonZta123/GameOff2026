using System;
using ECS.Entities;
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
        private TileBase evacuationZone;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Marker marker;

        [SerializeField]
        private Sprite mine;

        [SerializeField]
        private Sprite money;

        [SerializeField]
        private float speed = 4f;

        [SerializeField]
        private float amplitude = 0.25f;

        public void SetCurrentPlayerType(PlayerType value)
        {
            marker.SetUsesArrow(false);
            _playerType = value;
            var color = Color.white;

            if (_entity._triggerType == TriggerType.HackerZone)
            {
                if (_playerType == PlayerType.Hacker)
                {
                    marker.SetText("Hold E to hack Computer!");
                    marker.SetUsesArrow(true);
                }

                color = Color.blue;
            }

            if (_entity._triggerType == TriggerType.DemolitionZone)
            {
                if (_playerType == PlayerType.Demolition)
                {
                    marker.SetText("Press E to place TNT!");
                    marker.SetUsesArrow(true);
                }

                color = Color.red;
            }

            if (_entity._triggerType == TriggerType.Exit)
            {
                marker.SetText("Wait here to evacuate!");
                marker.SetUsesArrow(true);
                color = Color.green;
            }

            if (_entity._triggerType == TriggerType.Mine && _playerType == PlayerType.Demolition)
            {
                color = Color.red;
            }

            var size = Math.Max(_entity._rightTop.x - _entity._leftBottom.x,
                _entity._rightTop.y - _entity._leftBottom.y);

            var position = (_entity._leftBottom + _entity._rightTop) / 2f;

            marker.SetBaseSize(size, color);
            marker.SetPosition(position);
        }

        private PlayerType _playerType;

        public void Init(ECS.Entities.Trigger entity)
        {
            marker.SetUsesArrow(false);
            if (entity._triggerType is TriggerType.HackerZone or TriggerType.DemolitionZone or TriggerType.Exit)
            {
                var sizeX = entity._rightTop.x - entity._leftBottom.x;
                var sizeY = entity._rightTop.y - entity._leftBottom.y;

                var tile = entity._triggerType switch
                {
                    TriggerType.HackerZone => hackerZone,
                    TriggerType.DemolitionZone => demolitionZone,
                    TriggerType.Exit => evacuationZone,
                    _ => null
                };

                for (var i = 0; i < sizeX; i++)
                for (var j = 0; j < sizeY; j++)
                    tilemap.SetTile(new Vector3Int((int)entity._leftBottom.x + i, (int)entity._leftBottom.y + j, 0),
                        tile);
            }
            else if (entity._triggerType == TriggerType.Mine)
            {
                spriteRenderer.sprite = mine;
                transform.position = (entity._leftBottom + entity._rightTop) / 2f;
                marker.SetText("Optional: E to defuse!");
            }
            else if (entity._triggerType == TriggerType.Money)
            {
                spriteRenderer.sprite = money;
                _oscillate = true;
                transform.position = (entity._leftBottom + entity._rightTop) / 2f;
                marker.SetText("Collect Money!");
            }

            _entity = entity;
        }

        private ECS.Entities.Trigger _entity;
        private bool _oscillate;

        private void Update()
        {
            if (!_oscillate)
                return;

            spriteRenderer.transform.localPosition =
                Vector2.up * (amplitude * Mathf.Sin(Time.time * speed + transform.position.x));
        }

        public void SetState(ECS.Entities.Trigger entity, int tick)
        {
        }

        public void Release()
        {
            _oscillate = false;
            spriteRenderer.sprite = null;
            spriteRenderer.transform.localPosition = Vector2.zero;
            transform.position = Vector2.zero;
            marker.SetUsesArrow(false);
            tilemap.ClearAllTiles();
        }
    }
}

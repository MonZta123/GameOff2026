using System.Collections.Generic;
using UnityEngine;

namespace ECS
{
    public enum ScenarioType
    {
        Hack,
        Demolition,
        Grab,
        Exit
    }

    public struct MapDefinition
    {
        public int[] _map;

        public int _width;
        public int _height;

        public ScenarioType _type;
        public Vector2Int[] _spawnPoints;
        public Vector2Int[] _hackerZone;
        public Vector2Int[] _segmentDoor;
        public Vector2Int[] _demolitionZone;
        public Vector2Int[] _exit;
        public Vector2Int[] _woodenDoors;
        public Vector2Int[] _computerAsset;

        public int CoordToIndex(int x, int y)
        {
            return y * _width + x;
        }
    }

    public static class TextureMapBuilder
    {
        public static MapDefinition GetMap(Texture2D mapTexture)
        {
            var map = new MapDefinition();
            var result = new int[mapTexture.width * mapTexture.height];

            map._map = result;
            map._width = mapTexture.width;
            map._height = mapTexture.height;

            map._spawnPoints = new Vector2Int[4];

            var woodenDoors = new List<Vector2Int>();
            var segmentDoor = new List<Vector2Int>();
            var hackerZone = new List<Vector2Int>();
            var computerAsset = new List<Vector2Int>();
            var demolitionZone = new List<Vector2Int>();
            var exit = new List<Vector2Int>();

            var pixelMap = mapTexture.GetPixels32();

            for (var i = 0; i < mapTexture.height; i++)
            {
                for (var j = 0; j < mapTexture.width; j++)
                {
                    var color = pixelMap[i * mapTexture.width + j];

                    var tile = ColorToTile(color);

                    result[i * mapTexture.width + j] = 0;

                    if (tile is 0)
                        continue;

                    result[i * mapTexture.width + j] = 1;
                    switch (tile)
                    {
                        case 2 or 3 or 4 or 5:
                            map._spawnPoints[tile - 2] = new Vector2Int(j, i);
                            break;

                        case 6:
                            woodenDoors.Add(new Vector2Int(j, i));
                            break;

                        case 7:
                            segmentDoor.Add(new Vector2Int(j, i));
                            break;

                        case 8:
                            hackerZone.Add(new Vector2Int(j, i));
                            break;

                        case 9:
                            computerAsset.Add(new Vector2Int(j, i));
                            break;

                        case 10:
                            demolitionZone.Add(new Vector2Int(j, i));
                            break;

                        case 11:
                            exit.Add(new Vector2Int(j, i));
                            break;
                    }
                }
            }

            map._woodenDoors = woodenDoors.ToArray();
            map._hackerZone = hackerZone.ToArray();
            map._demolitionZone = demolitionZone.ToArray();
            map._segmentDoor = segmentDoor.ToArray();
            map._computerAsset = computerAsset.ToArray();
            map._exit = exit.ToArray();

            return map;
        }

        private static int ColorToTile(Color32 color)
        {
            if (color is { r: 0, g: 0, b: 0 }) // Wall
                return 0;

            if (color is { r: 255, g: 255, b: 255 }) // Floor
                return 1;

            if (color is { r: 255, g: 0, b: 0 }) // Heavy
                return 2;

            if (color is { r: 0, g: 255, b: 0 }) // Hacker
                return 3;

            if (color is { r: 255, g: 255, b: 0 }) // Demolition
                return 4;

            if (color is { r: 0, g: 0, b: 255 }) // Sniper
                return 5;

            if (color is { r: 185, g: 122, b: 87 }) // Wooden Door
                return 6;

            if (color is { r: 127, g: 127, b: 127 }) // 1. Segment door
                return 7;

            if (color is { r: 163, g: 73, b: 164 }) // Hacker Zone
                return 8;

            if (color is { r: 0, g: 162, b: 232 }) // Computer Asset
                return 9;

            if (color is { r: 255, g: 127, b: 39 }) // Demolition Zone
                return 10;

            if (color is { r: 181, g: 230, b: 29 }) // Segment 4 Exit
                return 11;

            return -1;
        }
    }
}

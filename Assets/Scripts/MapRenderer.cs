using System.Collections.Generic;
using ECS;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private List<TileBase> border;

    [SerializeField]
    private TileBase outside;

    [SerializeField]
    private TileBase[] visualWall;

    [SerializeField]
    private TileBase[] floor;

    public void GenerateMap(MapDefinition mapDefinition)
    {
        tilemap.ClearAllTiles();

        var floorTile = floor[mapDefinition._isInside ? 1 : 0];

        for (var i = -10; i < 0; i++)
        {
            for (var j = -10; j < mapDefinition._height + 10; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), outside);
                tilemap.SetTile(new Vector3Int(i + mapDefinition._width + 10, j, 0), outside);
            }
        }

        for (var i = 0; i < mapDefinition._width; i++)
        {
            for (var j = -10; j < 0; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), outside);
                tilemap.SetTile(new Vector3Int(i, j + mapDefinition._height + 10, 0), outside);
            }
        }

        for (var i = 0; i < mapDefinition._width; i++)
        {
            for (var j = 0; j < mapDefinition._height; j++)
            {
                var value = mapDefinition._map[j * mapDefinition._width + i];

                if (value == 0)
                {
                    if (OutsideRule(i, j, mapDefinition))
                        continue;

                    if (LeftTopCornerRule(i, j, mapDefinition))
                        continue;

                    if (LeftBottomCornerRule(i, j, mapDefinition))
                        continue;

                    if (RightTopCornerRule(i, j, mapDefinition))
                        continue;

                    if (RightBottomCornerRule(i, j, mapDefinition))
                        continue;

                    if (TopRule(i, j, mapDefinition))
                        continue;

                    if (RightRule(i, j, mapDefinition))
                        continue;

                    if (BottomRule(i, j, mapDefinition))
                        continue;

                    if (LeftRule(i, j, mapDefinition))
                        continue;

                    if (RightBottomCornerOutsideRule(i, j, mapDefinition))
                        continue;

                    if (LeftBottomCornerOutsideRule(i, j, mapDefinition))
                        continue;

                    if (LeftTopCornerOutsideRule(i, j, mapDefinition))
                        continue;

                    if (RightTopCornerOutsideRule(i, j, mapDefinition))
                        continue;
                }
                else
                {
                    if (TopIsWallRule(i, j, mapDefinition))
                        continue;

                    tilemap.SetTile(new Vector3Int(i, j, 0), floorTile);
                }
            }
        }
    }

    private bool TopIsWallRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (j != map._height - 1 && a[map.CoordToIndex(i, j + 1)] == 0)
        {
            if (i != 0 && a[map.CoordToIndex(i - 1, j + 1)] == 1)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[0 + (map._isInside ? 3 : 0)]);
                return true;
            }

            if (i != map._width - 1 && a[map.CoordToIndex(i + 1, j + 1)] == 1)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[2 + (map._isInside ? 3 : 0)]);
                return true;
            }

            tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[1 + (map._isInside ? 3 : 0)]);
            return true;
        }

        return false;
    }

    private bool OutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) &&
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) &&
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) &&
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) &&
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) &&
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) &&
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) &&
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0)
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), outside);
            return true;
        }

        return false;
    }

    private bool RightBottomCornerRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 1) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[9 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool RightBottomCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 1) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 1) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 1) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[2 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftBottomCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 1) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 1) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 1) // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 1) && // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[3 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftBottomCornerRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 1) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[11 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftTopCornerRule(int i, int j, MapDefinition map)
    {
        var a = map._map;


        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 1) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[6 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftTopCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 1) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 1) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 1) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 1) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[1 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool RightTopCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 1) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 1) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 1) //&& // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[0 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }


    private bool RightTopCornerRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 1) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[4 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool TopRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 1) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) // && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 1) && // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 1) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[10 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool BottomRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 1) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 1) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 1) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[5 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool RightRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 1) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 1) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 1) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[7 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 1) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 1) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 1) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[8 + (map._isInside ? 12 : 0)]);
            return true;
        }

        return false;
    }
}

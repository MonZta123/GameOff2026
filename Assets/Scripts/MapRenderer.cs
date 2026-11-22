using System;
using System.Collections.Generic;
using ECS;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapRenderer : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private List<TileBase> border;

    [SerializeField]
    private TileBase[] outside;

    [SerializeField]
    private TileBase[] visualWall;

    [SerializeField]
    private TileBase[] floor;

    [SerializeField]
    private TileBase[] tiledFloor;

    [SerializeField]
    private TileBase crackedFloor;

    [SerializeField]
    private TileBase[] doorSimpleOpen;

    [SerializeField]
    private TileBase[] doorSimpleClosed;

    [SerializeField]
    private TileBase[] doorDoubleOpen;

    [SerializeField]
    private TileBase[] doorDoubleClosed;

    [SerializeField]
    private TileBase[] objectiveAssets;

    private TileBase GetOutside()
    {
        if (Random.Range(0, 10) == 5)
            return outside[1];

        return outside[0];
    }

    private bool _previousIsDoor;

    public void GenerateMap(MapDefinition mapDefinition)
    {
        tilemap.ClearAllTiles();

        var floorTile = floor[(int)mapDefinition._scenery];

        for (var i = -10; i < 0; i++)
        {
            for (var j = -10; j < mapDefinition._height + 10; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), GetOutside());
                tilemap.SetTile(new Vector3Int(i + mapDefinition._width + 10, j, 0), GetOutside());
            }
        }

        for (var i = 0; i < mapDefinition._width; i++)
        {
            for (var j = -10; j < 0; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), GetOutside());
                tilemap.SetTile(new Vector3Int(i, j + mapDefinition._height + 10, 0), GetOutside());
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
                    if (value is 7)
                    {
                        var nextIsDoor = mapDefinition._map[j * mapDefinition._width + i + 1] == 7;

                        if (_previousIsDoor)
                        {
                            var offset = (int)mapDefinition._scenery * 2;
                            tilemap.SetTile(new Vector3Int(i, j, 0), doorDoubleClosed[1 + offset]);
                        }
                        else
                        {
                            if (nextIsDoor)
                            {
                                var offset = (int)mapDefinition._scenery * 2;
                                tilemap.SetTile(new Vector3Int(i, j, 0), doorDoubleClosed[0 + offset]);
                            }
                            else
                            {
                                var offset = (int)mapDefinition._scenery;
                                tilemap.SetTile(new Vector3Int(i, j, 0), doorSimpleClosed[0 + offset]);
                            }
                        }
                        
                        _previousIsDoor = nextIsDoor && !_previousIsDoor;

                        continue;
                    }

                    if (value is 9)
                    {
                        tilemap.SetTile(new Vector3Int(i, j, 0), objectiveAssets[0]);
                        continue;
                    }

                    if (value is 13)
                    {
                        tilemap.SetTile(new Vector3Int(i, j, 0), objectiveAssets[1]);
                        continue;
                    }

                    if (TopIsWallRule(i, j, mapDefinition))
                        continue;

                    if (mapDefinition._scenery == MapType.Outside && Random.Range(0, 10) == 5 && value is 1)
                    {
                        tilemap.SetTile(new Vector3Int(i, j, 0), crackedFloor);
                    }
                    else
                    {
                        if (value is 1 or 12 || mapDefinition._scenery == MapType.Safe)
                        {
                            tilemap.SetTile(new Vector3Int(i, j, 0), floorTile);
                        }

                        if (value is 1 or 12 && mapDefinition._scenery != MapType.Safe)
                        {
                            if (mapDefinition._scenery == MapType.Outside)
                                tilemap.SetTile(new Vector3Int(i, j, 0), tiledFloor[0]);
                            else
                                tilemap.SetTile(new Vector3Int(i, j, 0), tiledFloor[1]);
                        }
                    }
                }
            }
        }
    }

    private bool TopIsWallRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (j != map._height - 1 && a[map.CoordToIndex(i, j + 1)] == 0)
        {
            if (i != 0 && IsInNumber(a[map.CoordToIndex(i - 1, j + 1)]))
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[0 + (map._scenery != MapType.Outside ? 3 : 0)]);
                return true;
            }

            if (i != map._width - 1 && IsInNumber(a[map.CoordToIndex(i + 1, j + 1)]))
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[2 + (map._scenery != MapType.Outside ? 3 : 0)]);
                return true;
            }

            tilemap.SetTile(new Vector3Int(i, j, 0), visualWall[1 + (map._scenery != MapType.Outside ? 3 : 0)]);
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
            tilemap.SetTile(new Vector3Int(i, j, 0), GetOutside());
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
            (i == map._width - 1 || j == map._height - 1 ||
             IsInNumber(a[map.CoordToIndex(i + 1, j + 1)])) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[9 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool RightBottomCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j)])) && // Left
            (j == 0 || IsInNumber(a[map.CoordToIndex(i, j - 1)])) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j - 1)])) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[2 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private readonly List<int> _magicNumbers = new()
    {
        1,
        12,
        9,
        13,
        7
    };

    private bool IsInNumber(int number)
    {
        return _magicNumbers.Contains(number);
    }

    private bool LeftBottomCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || IsInNumber(a[map.CoordToIndex(i, j - 1)])) && // Up
            (i == map._width - 1 || IsInNumber(a[map.CoordToIndex(i + 1, j)])) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || IsInNumber(a[map.CoordToIndex(i + 1, j - 1)])) // Right Top
            // (i == 0 || j == map._height - 1 ||IsInNumber(a[map.CoordToIndex(i - 1, j + 1)])) && // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[3 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (i == 0 || j == map._height - 1 || IsInNumber(a[map.CoordToIndex(i - 1, j + 1)])) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[11 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (i == 0 || j == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j - 1)])) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[6 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (i == map._width - 1 || IsInNumber(a[map.CoordToIndex(i + 1, j)])) && // Right
            (j == map._height - 1 || IsInNumber(a[map.CoordToIndex(i, j + 1)])) && // Down
            // (i == 0 || j == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j - 1)])) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 ||
             IsInNumber(a[map.CoordToIndex(i + 1, j + 1)])) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[1 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool RightTopCornerOutsideRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j)])) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || IsInNumber(a[map.CoordToIndex(i, j + 1)])) && // Down
            // (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            // (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            (i == 0 || j == map._height - 1 || IsInNumber(a[map.CoordToIndex(i - 1, j + 1)])) //&& // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[0 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (i == map._width - 1 || j == 0 || IsInNumber(a[map.CoordToIndex(i + 1, j - 1)])) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[4 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (j == map._height - 1 || IsInNumber(a[map.CoordToIndex(i, j + 1)])) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) // && // Right Top
            // (i == 0 || j == map._height - 1 ||IsInNumber(a[map.CoordToIndex(i - 1, j + 1)])) && // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 ||IsInNumber(a[map.CoordToIndex(i + 1, j + 1)])) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[10 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool BottomRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || a[map.CoordToIndex(i - 1, j)] == 0) && // Left
            (j == 0 || IsInNumber(a[map.CoordToIndex(i, j - 1)])) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j - 1)])) && // Left Top
            // (i == map._width - 1 || j == 0 || IsInNumber(a[map.CoordToIndex(i + 1, j - 1)])) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[5 + (map._scenery != MapType.Outside ? 12 : 0)]);
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
            (i == map._width - 1 || IsInNumber(a[map.CoordToIndex(i + 1, j)])) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            (i == 0 || j == 0 || a[map.CoordToIndex(i - 1, j - 1)] == 0) && // Left Top
            // (i == map._width - 1 || j == 0 || IsInNumber(a[map.CoordToIndex(i + 1, j - 1)])) && // Right Top
            (i == 0 || j == map._height - 1 || a[map.CoordToIndex(i - 1, j + 1)] == 0) // Left Bottom
            // (i == map._width - 1 || j == map._height - 1 ||IsInNumber(a[map.CoordToIndex(i + 1, j + 1)])) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[7 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }

    private bool LeftRule(int i, int j, MapDefinition map)
    {
        var a = map._map;

        if (
            (i == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j)])) && // Left
            (j == 0 || a[map.CoordToIndex(i, j - 1)] == 0) && // Up
            (i == map._width - 1 || a[map.CoordToIndex(i + 1, j)] == 0) && // Right
            (j == map._height - 1 || a[map.CoordToIndex(i, j + 1)] == 0) && // Down
            // (i == 0 || j == 0 || IsInNumber(a[map.CoordToIndex(i - 1, j - 1)])) && // Left Top
            (i == map._width - 1 || j == 0 || a[map.CoordToIndex(i + 1, j - 1)] == 0) && // Right Top
            // (i == 0 || j == map._height - 1 ||IsInNumber(a[map.CoordToIndex(i - 1, j + 1)])) && // Left Bottom
            (i == map._width - 1 || j == map._height - 1 || a[map.CoordToIndex(i + 1, j + 1)] == 0) // Right Bottom
        )
        {
            tilemap.SetTile(new Vector3Int(i, j, 0), border[8 + (map._scenery != MapType.Outside ? 12 : 0)]);
            return true;
        }

        return false;
    }
}

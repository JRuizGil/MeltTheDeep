using UnityEngine;
using System.Collections.Generic;
public static class MarchingCubesTables
{
    public static readonly Vector3Int[] CornerOffsets = 
    {
        /* 8 esquinas, según la figura */
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1)
    };
    public static readonly int[,] EdgeCorners = 
    {
        /* 12 parejas de esquinas */
        {0, 1},
        {1, 2},
        {2, 3},
        {0, 3},
        {4, 5},
        {5, 6},
        {6, 7},
        {4, 7},
        {0, 4},
        {1, 5},
        {2, 6},
        {3, 7}
    };
/*
    public static void Build(IceVolume volume, Vector3Int cubeMin, Vector3Int cubeMaxExclusive,
    List<Vector3> vertices, List<int> triangles, float iso)
    {
        
    }
*/
}

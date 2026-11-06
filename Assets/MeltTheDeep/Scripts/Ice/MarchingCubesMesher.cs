using UnityEngine;
using System.Collections.Generic;

public static class MarchingCubesMesher 
{
/*
    static float[] density = new float[8];
    static Vector3Int[] CornerOffsets = new Vector3Int[8] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 1), new Vector3Int(0, 1, 1) };
    static Vector3Int[] EdgeOffsets = new Vector3Int[12] { new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(1, 0, 1), new Vector3Int(1, 1, 1), new Vector3Int(0, 1, 1),
        new Vector3Int(0, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(1, 1, 0), new Vector3Int(0, 1, 0) };
    static int[] EdgeTable = new int[256] {  Precomputed edge table values  };
    static int[,] TriTable = new int[256, 16] {  Precomputed triangle table values  };
    
    static IceVolume volume;



    public static void Build(IceVolume volume, Vector3Int cubeMin, Vector3Int cubeMaxExclusive,List<Vector3> vertices, List<int> triangles, float iso)
    {
        //for cz cy c en [cubemin, cubemaxExclusive)]
        for (int z = cubeMin.z; z < cubeMaxExclusive.z; z++)
        {
            for (int y = cubeMin.y; y < cubeMaxExclusive.y; y++)
            {
                for (int x = cubeMin.x; x < cubeMaxExclusive.x; x++)
                {
                    //dens[i]= volume.GetDensity(c + cornerOffsets[i])//8 lecturas a la vez
                                    
                    density[0] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[0]);
                    density[1] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[1]);
                    density[2] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[2]);
                    density[3] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[3]);
                    density[4] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[4]);
                    density[5] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[5]);
                    density[6] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[6]);
                    density[7] = volume.GetDensity(new Vector3Int(x, y, z) + CornerOffsets[7]);
                    //idx =  ComputeCubeIndex(dens, iso) //bit i si dens[i ] < iso
                    int idx = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (density[i] < iso)
                        {
                            idx |= 1 << i;
                        }
                    }
                    //mask = EdgeTable[idx]; si mask == 0 siguiente cubo
                    int mask = EdgeTable[idx];
                    if (mask == 0)
                    {
                        continue;
                    }
                    //para cada arista con bit activo interpolateedge edgever[e] , edgeNorm[e]
                    //for k = 0; tritable[idx][k] != -1; k+=3:
                    for (int k = 0; TriTable[idx, k] != -1; k += 3)
                    {
                        //b= vertices.count ; añade 3 vertices y su s normales
                        int b = vertices.Count;
                        //triangles += b+2 , b+1, b 
                        triangles.Add(b + 2);
                        triangles.Add(b + 1);
                        triangles.Add(b);

                    }


                }
            }
        }
    }
    static void ComputeCubeIndex(float[] dens, float iso, out int idx, out int mask)
    {
        idx = 0;
        for (int i = 0; i < 8; i++)
        {
            if (dens[i] < iso)
            {
                idx |= 1 << i;
            }
        }
        mask = EdgeTable[idx];
    }
    
    //interpolate edge : t = |dB-dA| < 1e-5 ? 0.5 : (iso-dA)/(dB-dA)
    static void InterpolateEdge(Vector3Int a, Vector3Int b, float dA, float dB, float iso, out Vector3 vertex, out Vector3 normal)
    {
        float t = Mathf.Abs(dB - dA) < 1e-5f ? 0.5f : (iso - dA) / (dB - dA);
        vertex = Vector3.Lerp(a, b, t);
        //
        normal = Vector3.Lerp(volume.GetNormal(a), volume.GetNormal(b), t).normalized;
    }
    //grad(p)  = (d(x+1)-d(y+1), d(x+1)-d(y+1), d(x+1)-d(y+1))
    static Vector3 ComputeGradient(Vector3Int p, IceVolume volume)
    {
        float dx = volume.GetDensity(p + new Vector3Int(1, 0, 0)) - volume.GetDensity(p + new Vector3Int(-1, 0, 0));
        float dy = volume.GetDensity(p + new Vector3Int(0, 1, 0)) - volume.GetDensity(p + new Vector3Int(0, -1, 0));
        float dz = volume.GetDensity(p + new Vector3Int(0, 0, 1)) - volume.GetDensity(p + new Vector3Int(0, 0, -1));
        return new Vector3(dx, dy, dz).normalized;
    }        
    */
}
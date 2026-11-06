using UnityEngine;
using System.Collections.Generic;

public static class MarchingCubesMesher 
{
    //acceso a IceVolume para obtener densidades y posiciones locales
    
    static float[] density = new float[8]; 
    static Vector3[] EdgeVertexPositions = new Vector3[12];
    static Vector3[] EdgeVertexNormals = new Vector3[12]; 
    //normals en List<Vector3>
    static List<Vector3> normals = new List<Vector3>();
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
                    //crea c una vez por cubo y lee las 8 esquinas con un for, sin repetir NewVector3Int 8 veces
                    Vector3Int c = new Vector3Int(x, y, z);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = c + MarchingCubesMesher.CornerOffsets[i];
                        density[i] = volume.GetDensity(corner);
                    }
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
                    int mask = MarchingCubesMesher.EdgeTable[idx];
                    if (mask == 0)
                    {
                        continue;
                    }
                    //para cada arista con bit activo interpolateedge edgever[e] , edgeNorm[e]
                    //bulce e= 0..11

                    for (int edge = 0; edge < 12; edge++)
                    {
                        if ((mask & (1 << edge)) != 0)
                        {
                            Vector3Int cornerA = new Vector3Int(x, y, z) + MarchingCubesMesher.CornerOffsets[EdgeCorners[edge, 0]];
                            Vector3Int cornerB = new Vector3Int(x, y, z) + MarchingCubesMesher.CornerOffsets[EdgeCorners[edge, 1]];
                            float dA = density[EdgeCorners[edge, 0]];
                            float dB = density[EdgeCorners[edge, 1]];
                            InterpolateEdge(cornerA, cornerB, dA, dB, iso, out Vector3 vertexPos, out Vector3 vertexNormal);
                            //almacena vertexPos y vertexNormal en un buffer estatico Vector3[12]
                            EdgeVertexPositions[edge] = vertexPos;
                            EdgeVertexNormals[edge] = vertexNormal;
                        }
                    }

                    //for k = 0; tritable[idx][k] != -1; k+=3:
                    //bucle de Tritable[idx] que devuelve 3 indices de aristas para cada triangulo
                    

                    for (int k = 0; MarchingCubesMesher.TriTable[idx, k] != -1; k += 3)
                    {
                        //b= vertices.count ; añade 3 vertices y su s normales
                        int b = vertices.Count;
                        //triangles += b+2 , b+1, b 
                        triangles.Add(b + 2);
                        triangles.Add(b + 1);
                        triangles.Add(b);
                        //añadir 3 vertices interpolados
                        for (int i = 0; i < 3; i++)
                        {
                            int edgeIndex = MarchingCubesMesher.TriTable[idx, k + i];
                            Vector3Int cornerA = new Vector3Int(x, y, z) + MarchingCubesMesher.CornerOffsets[EdgeCorners[edgeIndex, 0]];
                            Vector3Int cornerB = new Vector3Int(x, y, z) + MarchingCubesMesher.CornerOffsets[EdgeCorners[edgeIndex, 1]];
                            float dA = density[EdgeCorners[edgeIndex, 0]];
                            float dB = density[EdgeCorners[edgeIndex, 1]];
                            InterpolateEdge(cornerA, cornerB, dA, dB, iso, out Vector3 vertexPos, out Vector3 vertexNormal);
                            vertices.Add(vertexPos);
                            normals.Add(vertexNormal);
                        }
                        
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
        mask = MarchingCubesMesher.EdgeTable[idx];
    }
    
    //interpolate edge : t = |dB-dA| < 1e-5 ? 0.5 : (iso-dA)/(dB-dA)
    static void InterpolateEdge(Vector3Int a, Vector3Int b, float dA, float dB, float iso, out Vector3 vertex, out Vector3 normal)
    {
        //volume como primer parametro
        IceVolume volume = IceVolume.Instance;
        //tiene que escalar Lerp(a, b , t)  , da la posicion en indices de rejilla, la malla saldra 4 veces mas grande      
        float t = Mathf.Abs(dB - dA) < 1e-5f ? 0.5f : (iso - dA) / (dB - dA);
        vertex = Vector3.Lerp(a, b, t);
        //interpola entre volume.GetLocalPosition(a) y volume.GetLocalPosition(b) para obtener la posicion en el espacio local de la malla
        vertex = Vector3.Lerp(volume.GetLocalPosition(a), volume.GetLocalPosition(b), t);
        //normal = grad(a) * (1-t) + grad(b) * t
        Vector3 gradA = ComputeGradient(a, volume);
        Vector3 gradB = ComputeGradient(b, volume);
        normal = Vector3.Lerp(gradA, gradB, t).normalized;
        
    }
    //grad(p)  = (d(x+1)-d(y+1), d(x+1)-d(y+1), d(x+1)-d(y+1))
    static Vector3 ComputeGradient(Vector3Int p, IceVolume volume)
    {
        float dx = volume.GetDensity(p + new Vector3Int(1, 0, 0)) - volume.GetDensity(p + new Vector3Int(-1, 0, 0));
        float dy = volume.GetDensity(p + new Vector3Int(0, 1, 0)) - volume.GetDensity(p + new Vector3Int(0, -1, 0));
        float dz = volume.GetDensity(p + new Vector3Int(0, 0, 1)) - volume.GetDensity(p + new Vector3Int(0, 0, -1));
        return new Vector3(dx, dy, dz).normalized;
    }        
    
}
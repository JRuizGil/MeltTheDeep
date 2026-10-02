// <<hecho por IA>> 2026-10-02 · prototipo
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeltTheDeep
{
    /// <summary>
    /// Marching cubes en CPU. Recorre los cubos de un chunk y, en cada uno, mira qué esquinas son hielo,
    /// busca en la tabla los triángulos de ese caso y coloca sus vértices sobre las aristas,
    /// justo donde la densidad vale isoLevel.
    /// </summary>
    public class ChunkMesher
    {
        // Se reutilizan entre chunks para no generar basura en cada remallado.
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<int> triangles = new List<int>();
        readonly float[] cornerDensity = new float[8];
        readonly Vector3[] cornerPosition = new Vector3[8];

        /// <summary>Rellena 'mesh' con la superficie de los voxels [start, start + count).</summary>
        /// <returns>true si el chunk tiene superficie (al menos un triángulo).</returns>
        public bool Build(IceVolume volume, Vector3Int start, Vector3Int count, Mesh mesh)
        {
            vertices.Clear();
            triangles.Clear();

            for (int z = 0; z < count.z; z++)
                for (int y = 0; y < count.y; y++)
                    for (int x = 0; x < count.x; x++)
                        MarchCube(volume, start.x + x, start.y + y, start.z + z);

            mesh.Clear();
            mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return triangles.Count > 0;
        }

        void MarchCube(IceVolume volume, int x, int y, int z)
        {
            float iso = volume.IsoLevel;

            // 1. Índice del caso: un bit por cada esquina que está fuera del hielo (densidad < iso).
            int cubeIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                Vector3Int c = MarchingCubesTables.CornerOffsets[i];
                cornerDensity[i] = volume.GetDensity(x + c.x, y + c.y, z + c.z);
                cornerPosition[i] = volume.PointToLocal(x + c.x, y + c.y, z + c.z);
                if (cornerDensity[i] < iso) cubeIndex |= 1 << i;
            }

            // Todo hielo (0) o todo vacío (255): la superficie no pasa por este cubo.
            if (cubeIndex == 0 || cubeIndex == 255) return;

            // 2. La tabla da los triángulos del caso como ternas de aristas, terminadas en -1.
            for (int i = 0; MarchingCubesTables.Triangles[cubeIndex, i] != -1; i += 3)
            {
                // Se añaden en orden inverso (c, b, a) para que la cara visible mire hacia fuera
                // del hielo: Unity dibuja los triángulos cuyos vértices van en sentido horario.
                AddVertex(MarchingCubesTables.Triangles[cubeIndex, i + 2], iso);
                AddVertex(MarchingCubesTables.Triangles[cubeIndex, i + 1], iso);
                AddVertex(MarchingCubesTables.Triangles[cubeIndex, i], iso);
            }
        }

        // 3. Coloca el vértice en la arista, interpolando entre sus dos esquinas hasta donde la densidad vale iso.
        void AddVertex(int edge, float iso)
        {
            int a = MarchingCubesTables.EdgeCornerA[edge];
            int b = MarchingCubesTables.EdgeCornerB[edge];
            float t = (iso - cornerDensity[a]) / (cornerDensity[b] - cornerDensity[a]);

            triangles.Add(vertices.Count);
            vertices.Add(Vector3.Lerp(cornerPosition[a], cornerPosition[b], t));
        }
    }
}

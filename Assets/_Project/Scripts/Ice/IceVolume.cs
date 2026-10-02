// <<hecho por IA>> 2026-10-02 · prototipo
using System.Collections.Generic;
using UnityEngine;

namespace MeltTheDeep
{
    /// <summary>
    /// Bloque de hielo deformable. Guarda una rejilla 3D de densidad (0 = vacío, 1 = hielo sólido)
    /// y la reparte en chunks; cada chunk convierte su trozo de rejilla en malla con marching cubes.
    /// El objeto no debe tener escala: las medidas son en metros.
    /// </summary>
    public class IceVolume : MonoBehaviour
    {
        [Header("Tamaño")]
        [Tooltip("Espacio que llena el hielo, en metros, a partir de la posición de este objeto.")]
        [SerializeField] Vector3 size = new Vector3(10f, 6f, 10f);
        [Tooltip("Distancia entre puntos de la rejilla. Más pequeño = más detalle, pero más coste.")]
        [SerializeField, Min(0.05f)] float voxelSize = 0.25f;
        [Tooltip("Voxels por lado de cada chunk.")]
        [SerializeField, Min(2)] int chunkVoxels = 8;

        [Header("Malla")]
        [Tooltip("Valor de densidad en el que está la superficie del hielo.")]
        [SerializeField, Range(0.05f, 0.95f)] float isoLevel = 0.5f;
        [SerializeField] Material iceMaterial;
        [Tooltip("Máximo de chunks que se rehacen en un frame, para repartir el coste entre frames.")]
        [SerializeField, Min(1)] int maxRebuildsPerFrame = 8;

        float[] density;
        Vector3Int voxels;      // cubos de la rejilla por eje
        Vector3Int points;      // puntos por eje (= voxels + 1)
        Vector3 gridOrigin;     // posición local del punto (0, 0, 0)
        IceChunk[] chunks;
        Vector3Int chunkCount;
        ChunkMesher mesher;
        readonly Queue<IceChunk> dirtyChunks = new Queue<IceChunk>();

        public float IsoLevel => isoLevel;

        void Awake()
        {
            CreateGrid();
            CreateChunks();
            foreach (IceChunk chunk in chunks)
                chunk.Rebuild(mesher, this);
        }

        void LateUpdate()
        {
            // Se remalla una vez por frame, después de todos los fundidos del frame,
            // y como mucho maxRebuildsPerFrame chunks; el resto espera al frame siguiente.
            for (int i = 0; i < maxRebuildsPerFrame && dirtyChunks.Count > 0; i++)
                dirtyChunks.Dequeue().Rebuild(mesher, this);
        }

        /// <summary>
        /// Funde hielo alrededor de un punto: resta densidad a los puntos de la rejilla que están a menos
        /// de 'radius' metros, más en el centro que en el borde. Devuelve los litros de agua fundidos.
        /// </summary>
        public float Melt(Vector3 worldPoint, float radius, float amount)
        {
            // Centro y radio en unidades de la rejilla (1 = un voxel).
            Vector3 center = (transform.InverseTransformPoint(worldPoint) - gridOrigin) / voxelSize;
            float r = radius / voxelSize;

            // Caja de puntos que puede tocar la esfera, sin tocar el borde de la rejilla (siempre vale 0).
            Vector3Int min = Vector3Int.Max(Vector3Int.FloorToInt(center - Vector3.one * r), Vector3Int.one);
            Vector3Int max = Vector3Int.Min(Vector3Int.CeilToInt(center + Vector3.one * r), points - Vector3Int.one * 2);
            if (min.x > max.x || min.y > max.y || min.z > max.z) return 0f;

            float removed = 0f;
            for (int z = min.z; z <= max.z; z++)
                for (int y = min.y; y <= max.y; y++)
                    for (int x = min.x; x <= max.x; x++)
                    {
                        float distance = Vector3.Distance(new Vector3(x, y, z), center);
                        if (distance >= r) continue;

                        // Caída suave (smoothstep): funde del todo en el centro y nada en el borde de la esfera.
                        float falloff = Mathf.SmoothStep(1f, 0f, distance / r);
                        int i = Index(x, y, z);
                        float before = density[i];
                        density[i] = Mathf.Max(0f, before - amount * falloff);
                        removed += before - density[i];
                    }

            if (removed > 0f) MarkDirty(min, max);

            // Cada punto representa un cubo de voxelSize³ metros cúbicos, y 1 m³ son 1000 litros.
            return removed * voxelSize * voxelSize * voxelSize * 1000f;
        }

        // Pone en la cola los chunks que usan algún punto de [minPoint, maxPoint]. Un punto lo comparten
        // el voxel de su izquierda y el de su derecha, por eso se miran los voxels de minPoint - 1 a maxPoint:
        // así también se rehace el chunk vecino cuando el cambio cae justo en la frontera.
        void MarkDirty(Vector3Int minPoint, Vector3Int maxPoint)
        {
            Vector3Int from = ChunkOfVoxel(minPoint - Vector3Int.one);
            Vector3Int to = ChunkOfVoxel(maxPoint);

            for (int z = from.z; z <= to.z; z++)
                for (int y = from.y; y <= to.y; y++)
                    for (int x = from.x; x <= to.x; x++)
                    {
                        IceChunk chunk = chunks[x + chunkCount.x * (y + chunkCount.y * z)];
                        if (chunk.IsDirty) continue; // ya está en la cola
                        chunk.IsDirty = true;
                        dirtyChunks.Enqueue(chunk);
                    }
        }

        // Chunk al que pertenece un voxel. El último chunk de cada eje se queda con los voxels sobrantes.
        Vector3Int ChunkOfVoxel(Vector3Int voxel) => new Vector3Int(
            Mathf.Clamp(voxel.x / chunkVoxels, 0, chunkCount.x - 1),
            Mathf.Clamp(voxel.y / chunkVoxels, 0, chunkCount.y - 1),
            Mathf.Clamp(voxel.z / chunkVoxels, 0, chunkCount.z - 1));

        void CreateGrid()
        {
            // La superficie queda a medio voxel de los puntos del borde, así que la rejilla sobresale
            // medio voxel por cada lado: de este modo el hielo llena exactamente 'size'.
            voxels = new Vector3Int(
                Mathf.RoundToInt(size.x / voxelSize) + 1,
                Mathf.RoundToInt(size.y / voxelSize) + 1,
                Mathf.RoundToInt(size.z / voxelSize) + 1);
            points = voxels + Vector3Int.one;
            gridOrigin = -0.5f * voxelSize * Vector3.one;

            density = new float[points.x * points.y * points.z];
            for (int z = 0; z < points.z; z++)
                for (int y = 0; y < points.y; y++)
                    for (int x = 0; x < points.x; x++)
                    {
                        // Los puntos del borde valen 0 para que la malla quede cerrada por todos los lados.
                        bool border = x == 0 || y == 0 || z == 0 ||
                                      x == points.x - 1 || y == points.y - 1 || z == points.z - 1;
                        density[Index(x, y, z)] = border ? 0f : 1f;
                    }
        }

        void CreateChunks()
        {
            mesher = new ChunkMesher();

            // Chunks enteros por eje; el último de cada eje se queda con los voxels que sobran.
            chunkCount = new Vector3Int(
                Mathf.Max(1, voxels.x / chunkVoxels),
                Mathf.Max(1, voxels.y / chunkVoxels),
                Mathf.Max(1, voxels.z / chunkVoxels));
            chunks = new IceChunk[chunkCount.x * chunkCount.y * chunkCount.z];

            for (int z = 0; z < chunkCount.z; z++)
                for (int y = 0; y < chunkCount.y; y++)
                    for (int x = 0; x < chunkCount.x; x++)
                    {
                        Vector3Int start = new Vector3Int(x, y, z) * chunkVoxels;
                        Vector3Int end = new Vector3Int(
                            x == chunkCount.x - 1 ? voxels.x : start.x + chunkVoxels,
                            y == chunkCount.y - 1 ? voxels.y : start.y + chunkVoxels,
                            z == chunkCount.z - 1 ? voxels.z : start.z + chunkVoxels);

                        var go = new GameObject($"IceChunk {x},{y},{z}");
                        go.layer = gameObject.layer;
                        go.transform.SetParent(transform, false);

                        IceChunk chunk = go.AddComponent<IceChunk>();
                        chunk.Init(start, end - start, iceMaterial);
                        chunks[x + chunkCount.x * (y + chunkCount.y * z)] = chunk;
                    }
        }

        public float GetDensity(int x, int y, int z) => density[Index(x, y, z)];

        public Vector3 PointToLocal(int x, int y, int z) => gridOrigin + new Vector3(x, y, z) * voxelSize;

        // La rejilla 3D se guarda en un array 1D: primero avanza x, luego y, luego z.
        int Index(int x, int y, int z) => x + points.x * (y + points.y * z);

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(size * 0.5f, size);
        }
    }
}

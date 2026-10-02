// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;

namespace MeltTheDeep
{
    /// <summary>
    /// Un trozo del bloque de hielo: su malla, su render y su collider.
    /// No guarda densidad; la lee de IceVolume cada vez que hay que rehacer la malla.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class IceChunk : MonoBehaviour
    {
        public Vector3Int StartVoxel { get; private set; }
        public Vector3Int VoxelCount { get; private set; }
        /// <summary>true mientras espera en la cola de IceVolume para rehacer su malla.</summary>
        public bool IsDirty { get; set; }

        Mesh mesh;
        MeshCollider meshCollider;

        public void Init(Vector3Int startVoxel, Vector3Int voxelCount, Material material)
        {
            StartVoxel = startVoxel;
            VoxelCount = voxelCount;

            mesh = new Mesh { name = name };
            mesh.MarkDynamic(); // avisa a Unity de que esta malla se va a reescribir a menudo
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            meshCollider = GetComponent<MeshCollider>();
        }

        public void Rebuild(ChunkMesher mesher, IceVolume volume)
        {
            bool hasSurface = mesher.Build(volume, StartVoxel, VoxelCount, mesh);

            // Volver a asignar la malla obliga al collider a recalcularse con la forma nueva.
            meshCollider.sharedMesh = null;
            if (hasSurface) meshCollider.sharedMesh = mesh;
            IsDirty = false;
        }

        void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
        }
    }
}

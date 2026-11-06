using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IceVolume : MonoBehaviour
{
    #region Configurables Volumen
    [Header("Configurar volumen del hielo")]      
    [SerializeField] private Vector3 size = new Vector3(10f, 6f, 10f);
    //
    public const float IsoLevel = 0.5f;

    [Tooltip("tamaño de los voxeles")]
    [SerializeField] private float voxelSize = 0.25f;
    [SerializeField, Range(0f, 6f)] float iceHeight = 4.5f;
    public enum FillMode { Pool, Sphere }

    [SerializeField] FillMode fillMode = FillMode.Pool;
    private float [] density;

    public Vector3Int PointCount { get; private set; }
    public Vector3Int CellCount { get; private set; }
    
    [SerializeField] private int gizmoSliceY = 0;

    private GameObject meshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh iceMesh;
    [SerializeField]private Material iceMaterial;
    //array de triangles y vertices para la malla
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> triangles = new List<int>();

    #endregion

    // Initializes the density array based on the size and voxel size of the ice volume
    [ContextMenu("Generate Ice")]
    public void Initialize()
    {
        //CellCount = RoundToInt(size / voxelSize) en cada eje, //40,24, 40
        CellCount = new Vector3Int(
            Mathf.RoundToInt(size.x / voxelSize),
            Mathf.RoundToInt(size.y / voxelSize),
            Mathf.RoundToInt(size.z / voxelSize)
        );
        PointCount = CellCount + Vector3Int.one; //41, 25, 41
        density = new float[PointCount.x * PointCount.y * PointCount.z]; //la unica dimension del array es la cantidad de puntos, no de celdas
        Debug.Log($"rejilla {PointCount.x}x{PointCount.y}x{PointCount.z} = {density.Length} puntos");
        for (int z = 0; z < PointCount.z; z++)
        {
            for (int y = 0; y < PointCount.y; y++)
            {
                for (int x = 0; x < PointCount.x; x++)
                {
                    // Borde = alguna coordenada es 0 o su máximo
                    bool borde = x == 0 || y == 0 || z == 0 || x == PointCount.x - 1 || y == PointCount.y - 1 || z == PointCount.z - 1;
                    density[Index(x, y, z)] = borde ? 0f : Relleno(x, y, z);
                }
            }
        }        
    }

    float Relleno(int x, int y, int z)
    {
        // change fill depending on the shape from enum
        switch (fillMode)
        {   
            case FillMode.Pool:
                return Mathf.Clamp01(0.5f + (iceHeight - y * voxelSize) / voxelSize);
            case FillMode.Sphere:
                Vector3 p = new Vector3(x, y, z) * voxelSize;
                //restarle 2 * voxelSize
                float radius = Mathf.Min(size.x, size.y, size.z) * 0.5f - 2 * voxelSize;
                return Mathf.Clamp01(0.5f + (radius - Vector3.Distance(p, size * 0.5f)) / voxelSize);
            default:
                return 1f;
        }
    }

    void Awake()
    {
        Initialize();
        CreateMeshObject();
        //Crea un new Mesh() con indexFormat = UInt32 para poder tener más de 65535 vertices
        
        vertices.Clear();
        normals.Clear();
        triangles.Clear();
        RebuildMesh();
        
        
    }
    #region Ice generation Methods
    // Returns the index of the density array for the given point coordinates
    public int Index(int x, int y, int z)
    {
        return x + PointCount.x * (y + PointCount.y * z);
    }
    // Checks if the given point coordinates are within the bounds of the density array
    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < PointCount.x && y >= 0 && y < PointCount.y && z >= 0 && z < PointCount.z;
    }
    // Returns the local position of the given point coordinates in the ice volume
    public Vector3 GetLocalPosition(int x, int y, int z)
    {
        return new Vector3(x, y, z) * voxelSize;
    }
    //sobrecarga getlocalposition
    public Vector3 GetLocalPosition(Vector3Int point)
    {
        return new Vector3(point.x, point.y, point.z) * voxelSize;
    }
    // Returns the closest point coordinates in the ice volume for the given local position
    public Vector3Int GetClosestPoint(Vector3 localPosition)
    {
        return new Vector3Int(
            Mathf.RoundToInt(localPosition.x / voxelSize),
            Mathf.RoundToInt(localPosition.y / voxelSize),
            Mathf.RoundToInt(localPosition.z / voxelSize)
        );
    }
    // Gets the density value at the given point coordinates

    //añadr una sobrecarga en IceVolume.cs para que GetDensity acepte un Vector3Int como argumento y llame a la versión existente de GetDensity con los componentes x, y, z del Vector3Int. Esto permitirá obtener la densidad de un punto usando un Vector3Int en lugar de tres enteros separados.
    public float GetDensity(Vector3Int point)
    {
        return GetDensity(point.x, point.y, point.z);
    }
    
    public float GetDensity(int x, int y, int z)
    {
        if (InBounds(x, y, z))
        {
            return density[Index(x, y, z)];
        }
        return 0f;
    }    
    public void SetDensity(int x, int y, int z, float value)
    {
        //limit the value to be between 0 and 1
        value = Mathf.Clamp01(value);
        // Set the density value at the given point coordinates if they are within bounds
        if (InBounds(x, y, z))
        {
            density[Index(x, y, z)] = value;
        }
    }
#endregion
    // Draws a wireframe cube representing the ice volume in the scene view
    void OnDrawGizmosSelected()
    {
        //draw the wireframe cube representing the ice volume in the scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.TransformPoint(size * 0.5f), size);
        GizmoSliceY(gizmoSliceY);      
        
    }    
    void GizmoSliceY(int y)
    {
        //draw a small cube for each point in the density array at the given y level, with a color that depends on the density value
        for (int x = 0; x < PointCount.x; x++)
        {
            for (int z = 0; z < PointCount.z; z++)
            {                
                float densityValue = GetDensity(x, y, z);
                Color color = Color.Lerp(Color.blue, Color.red, Mathf.Clamp01(densityValue));
                Gizmos.color = color;
                Vector3 pointPosition = transform.TransformPoint(new Vector3(x * voxelSize, y * voxelSize, z * voxelSize));
                Gizmos.DrawCube(pointPosition, Vector3.one * voxelSize * 0.5f);                
            }
        }
    }
    //un método que crea un hijo con `MeshFilter`, `MeshRenderer` (material `M_Hielo`, URP/Lit celeste claro, Smoothness alto, opaco) y `MeshCollider`;
    
    void CreateMeshObject()
    {
        // crea un hijo con MeshFilter, MeshRenderer y MeshCollider
        //material M_Hielo, URP/Lit celeste claro, Smoothness alto, opaco

        meshObject = new GameObject("IceMesh");
        meshObject.transform.SetParent(transform);
        meshObject.transform.localPosition = Vector3.zero;
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshCollider = meshObject.AddComponent<MeshCollider>();  

        if (iceMaterial != null)
        {
            meshRenderer.material = iceMaterial;
        }
        else
        {
            Debug.LogWarning("Material 'M_Hielo' not found in Resources.");
        }

        iceMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        meshFilter.sharedMesh = iceMesh;
        
    }
    void RebuildMesh()
    {
        //1. vaciar las 3 listas (Clear)
        vertices.Clear();
        normals.Clear();
        triangles.Clear();              
        //asigna meshcollider.sharedMesh = meshFilter.mesh; sino el jugador se hude        
        MarchingCubesMesher.Build(this, Vector3Int.zero, CellCount, vertices, normals, triangles, IsoLevel);
        iceMesh.Clear();     
        iceMesh.SetVertices(vertices);
        iceMesh.SetNormals(normals);
        iceMesh.SetTriangles(triangles, 0);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = iceMesh;
    }
    
    
}

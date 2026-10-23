using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

public class IceVolume : MonoBehaviour
{
    #region Configurables Volumen
    [Header("Configurar volumen del hielo")]
      
    [SerializeField] public Vector3 size = new Vector3(10f, 6f, 10f);
    [SerializeField] public const float isoLevel = 0.5f;
    [Tooltip("tamaño de los voxeles")]
    [SerializeField] public float voxelSize = 0.25f;
    [SerializeField, Range(0f, 6f)] float iceHeight = 4.5f;
    public enum FillMode { Pool, Sphere }
    [SerializeField] FillMode fillMode = FillMode.Pool;
    private float [] density;
    public Vector3Int PointCount { get; private set; }
    public Vector3Int CellCount { get; private set; }
    #endregion

    // Initializes the density array based on the size and voxel size of the ice volume
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
                float radius = Mathf.Min(size.x, size.y, size.z) * 0.5f;
                return Mathf.Clamp01(0.5f + (radius - Vector3.Distance(p, size * 0.5f)) / voxelSize);
            default:
                return 1f;
        }
    }

    void Awake()
    {
        Initialize();
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
    // Gets the density value at the given point coordinates
    public float GetDensity(int x, int y, int z)
    {
        if (InBounds(x, y, z))
        {
            return density[Index(x, y, z)];
        }
        return 0f;
    }
    // Sets the density value at the given point coordinates
    [ContextMenu("Generate Ice")]
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
        Gizmos.DrawWireCube(transform.position + size * 0.5f, size);
        GizmoSliceY(iceHeight);
    }    
    void GizmoSliceY(float y)
    {
        //draw a small cube for each point in the density array at the given y level, with a color that depends on the density value
        for (int x = 0; x < PointCount.x; x++)
        {
            for (int z = 0; z < PointCount.z; z++)
            {
                float densityValue = GetDensity(x, Mathf.RoundToInt(y / voxelSize), z);
                Color color = Color.Lerp(Color.blue, Color.red, densityValue);
                Gizmos.color = color;
                Vector3 pointPosition = transform.position + new Vector3(x * voxelSize, y, z * voxelSize);
                Gizmos.DrawCube(pointPosition, Vector3.one * voxelSize * 0.5f);

            }
        }
    }
    
}

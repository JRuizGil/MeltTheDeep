using UnityEngine;

public class IceVolume : MonoBehaviour
{
    #region Configurables
    [SerializeField] Vector3 size = new Vector3(10f, 6f, 10f);
    [SerializeField] float voxelSize = 0.25f;
    [SerializeField, Range(0f, 6f)] float iceHeight = 4.5f;
    float [] density;
    public Vector3Int PointCount { get; private set; }
    public Vector3Int CellCount { get; private set; }
#endregion

    void Awake()
    {
        PointCount = new Vector3Int(
            Mathf.RoundToInt(size.x / voxelSize) + 1,
            Mathf.RoundToInt(size.y / voxelSize) + 1,
            Mathf.RoundToInt(size.z / voxelSize) + 1
        );
        CellCount = PointCount - Vector3Int.one;
        density = new float[PointCount.x * PointCount.y * PointCount.z];
    }
    #region Ice generation Methods
    public int Index(int x, int y, int z)
    {
        return x + PointCount.x * (y + PointCount.y * z);
    }
    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < PointCount.x &&
               y >= 0 && y < PointCount.y &&
               z >= 0 && z < PointCount.z;
    }
    public float GetDensity(int x, int y, int z)
    {
        if (InBounds(x, y, z))
        {
            return density[Index(x, y, z)];
        }
        return 0f;
    }
    [ContextMenu("Generate Ice")]
    public void SetDensity(int x, int y, int z, float value)
    {
        if (InBounds(x, y, z))
        {
            density[Index(x, y, z)] = value;
        }
    }
#endregion
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(size * 0.5f, size);
    }
}

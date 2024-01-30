using System.Numerics;

namespace AKG1.Models;
public class Vertex
{
    public int Index { get; set; }
    public Vector3 Coordinates { get; set; }
    public float WCoordinate { get; set; } = 1;
    public Vector3? TextureCoordinates { get; set; }
    public Vector3? NormalVector { get; set; }
}


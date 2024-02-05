using AKG1.Models;
using System.Numerics;
using System.Globalization;

namespace AKG1;
internal class ObjFileParser
{
    public List<Vertex> _vertices = new();
    public List<Vector3> _normals = new();
    public List<Vector3> _textures = new();
    public List<Polygon> _faces = new();

    public List<Tuple<float, float>> _points = new();

    //const string filePath = "..\\..\\..\\smile.obj";

    public void ParseOBJFile(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ParseLine(line);
            }
        }
    }

    #region parsing
    private void ParseLine(string line)
    {
        var spaceIndex = line.IndexOf(' ');
        if (spaceIndex == -1)
        {
            return;
        }

        var lineType = line.Substring(0, spaceIndex);
        var inputValues = line.Substring(spaceIndex + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (lineType)
        {
            case "v":
                var vertex = ParseVertex(inputValues);
                _vertices.Add(vertex);
                break;
            case "vt":
                var textures = ParseTextures(inputValues);
                _textures.Add(textures);
                break;
            case "vn":
                var normalVector = ParseNormalVector(inputValues);
                _normals.Add(normalVector);
                break;
            case "f":
                var polygons = ParseTriangulatedPolygon(inputValues);
                _faces.AddRange(polygons);
                break;
        }
    }

    private Vertex ParseVertex(string[] inputValues)
    {
        Vertex vertex = new Vertex();

        vertex.Coordinates = new Vector3(float.Parse(inputValues[0], CultureInfo.InvariantCulture),
              float.Parse(inputValues[1], CultureInfo.InvariantCulture),
              float.Parse(inputValues[2], CultureInfo.InvariantCulture));

        if (inputValues.Length > 3)
        {
            vertex.WCoordinate = float.Parse(inputValues[3], CultureInfo.InvariantCulture);
        }

        return vertex;
    }

    private Vector3 ParseTextures(string[] inputValues)
    {
        var texture = new Vector3(float.Parse(inputValues[0], CultureInfo.InvariantCulture))
        {
            Y = inputValues.Length > 1 ? float.Parse(inputValues[1], CultureInfo.InvariantCulture) : 0,
            Z = inputValues.Length > 2 ? float.Parse(inputValues[2], CultureInfo.InvariantCulture) : 0
        };
        return texture;
    }

    public Vector3 ParseNormalVector(string[] inputValues)
    {
        return new Vector3(float.Parse(inputValues[0], CultureInfo.InvariantCulture),
            float.Parse(inputValues[1], CultureInfo.InvariantCulture),
            float.Parse(inputValues[2], CultureInfo.InvariantCulture));
    }

    public IEnumerable<Polygon> ParseTriangulatedPolygon(string[] inputValues)
    {
        var vertices = new List<Vertex>();
        var indexes = new List<int>();
        foreach (var inputValue in inputValues)
        {
            var vertexValues = inputValue.Split('/');

            var index = int.Parse(vertexValues[0]);
            indexes.Add(index);
            index = index < 0 ? _vertices.Count - Math.Abs(index) : index - 1;

            var vertex = _vertices[index];

            if (vertexValues.Length > 1 && vertexValues[1] != "")
            {
                index = int.Parse(vertexValues[1]);
                index = index < 0 ? _textures.Count - Math.Abs(index) : index - 1;
                vertex.TextureCoordinates = _textures[index];
            }

            if (vertexValues.Length > 2 && vertexValues[2] != "")
            {
                index = int.Parse(vertexValues[2]);
                index = index < 0 ? _normals.Count - Math.Abs(index) : index - 1;
                vertex.NormalVector = _normals[index];
            }
            vertices.Add(vertex);
        }

        return TriangulatePolygon(vertices.ToArray(), indexes.ToArray());
    }

    private IEnumerable<Polygon> TriangulatePolygon(Vertex[] vertices, int[] indexes)
    {
        if (vertices.Length == 3)
        {
            return new Polygon[]
            {
                new() { Vertices = vertices.ToList(), Indexes = indexes.ToList() }

            };
        }

        var polygons = new List<Polygon>();

        var innerVertices = new List<Vertex>();

        int index = 2;
        for (; index < vertices.Length; index += 2)
        {
            if (index % 2 == 0)
            {
                innerVertices.Add(vertices[index]);
                polygons.Add(new Polygon()
                {
                    Vertices = new List<Vertex> {
                        vertices[index - 2],
                        vertices[index - 1],
                        vertices[index]
                    }
                });
            }
        }

        if (vertices.Length % 2 == 0)
        {
            innerVertices.Add(vertices[^1]);
        }
        innerVertices.Add(vertices[0]);

        polygons.AddRange(TriangulatePolygon(innerVertices.ToArray(), indexes));
        return polygons;
    }

    #endregion
}



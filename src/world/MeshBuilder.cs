namespace KDR;

using System.Numerics;
using System.Text.RegularExpressions;

public static partial class MeshBuilder
{
    public static Mesh CreateRectangleMesh(int width, int height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        float normalizedWidth = width / MathF.Max(width, height);
        float normalizedHeight = height / MathF.Max(width, height);

        return new Mesh(
            new Vector3[] {new(-halfWidth, halfHeight, 0), new(halfWidth, halfHeight, 0), new(-halfWidth, -halfHeight, 0), new(halfWidth, -halfHeight, 0)},
            new Vector2[] {new(0, 0), new(normalizedWidth, 0), new(0, normalizedHeight), new(normalizedWidth, normalizedHeight)},
            new IndexedFace[] {new(0, 1, 2, 0, 1, 2), new(3, 2, 1, 3, 2, 1)}
        );
    }

	public static Mesh BuildFromFile(string path)
	{
        List<Vector3> vertexList = new();
        List<Vector2> textureList = new();
        List<IndexedFace> faceList = new();

        using StreamReader reader = File.OpenText(path);
        int lineNumber = 0;

        while (reader.ReadLine() is string line)
        {
            lineNumber++;
            string errorDetail = $"The input string '{line}' at line {lineNumber} of '{path}' was not in the correct format.\n";

            string[] segments = Space().Split(line);

            switch (segments[0])
            {
                case "v":
                    try
                    {
                        string[] coordinates = Comma().Split(segments[1]);
                        vertexList.Add(StringArrayToVec3(coordinates));
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            errorDetail + "Object-space vertex format: 'v [x],[y],[z]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            errorDetail + "Vertex coordinate must be a valid floating-point value."
                        , e);
                    }

                    break;

                case "t":
                    try
                    {
                        string[] textureCoordinates = Comma().Split(segments[1]);
                        textureList.Add(StringArrayToVec2(textureCoordinates));
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            errorDetail + "Texture vertex format: 't [u],[v]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            errorDetail + "Texture coordinate must be a valid floating-point value."
                        , e);
                    }

                    break;

                case "f":
                    try
                    {
                        MatchCollection indexMatches = CaptureIndices().Matches(segments[1]);

                        IndexedFace Face = new(
                            Convert.ToInt32(indexMatches[0].Groups[1].Value),
                            Convert.ToInt32(indexMatches[1].Groups[1].Value),
                            Convert.ToInt32(indexMatches[2].Groups[1].Value),
                            
                            Convert.ToInt32(indexMatches[0].Groups[2].Value),
                            Convert.ToInt32(indexMatches[1].Groups[2].Value),
                            Convert.ToInt32(indexMatches[2].Groups[2].Value)
                        );

                        faceList.Add(Face);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            errorDetail + "Indexed face format: 'f [v index]/[t index],[v index]/[t index],[v index]/[t index]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            errorDetail + "Vertex and texture indices must be valid integer values."
                        , e);
                    }

                    break;
            }
        }

        return new Mesh(vertexList.ToArray(), textureList.ToArray(), faceList.ToArray());
    }

	static Vector2 StringArrayToVec2(string[] arr) => new(
		Convert.ToSingle(arr[0]),
		Convert.ToSingle(arr[1])
	);

	static Vector3 StringArrayToVec3(string[] arr) => new(
		Convert.ToSingle(arr[0]),
		Convert.ToSingle(arr[1]),
		Convert.ToSingle(arr[2])
	);

	[GeneratedRegex(@"\s")]
	private static partial Regex Space();

	[GeneratedRegex(",")]
	private static partial Regex Comma();

	[GeneratedRegex("([0-9]+)/([0-9]+)")]
	private static partial Regex CaptureIndices();
}

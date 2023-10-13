using System.Numerics;
using System.Text.RegularExpressions;

public static partial class MeshBuilder
{
	public static Mesh BuildFromFile(string Path)
	{
        List<Vector3> VertexList = new();
        List<Vector2> TextureList = new();
        List<IndexedFace> FaceList = new();

        using StreamReader Reader = File.OpenText(Path);
        int LineNumber = 0;

        while (Reader.ReadLine() is string Line)
        {
            LineNumber++;
            string ErrorDetail = $"The input string '{Line}' at line {LineNumber} of '{Path}' was not in the correct format.\n";

            string[] Segments = Space().Split(Line);

            switch (Segments[0])
            {
                case "v":
                    try
                    {
                        string[] Coordinates = Comma().Split(Segments[1]);
                        VertexList.Add(StringArrayToVec3(Coordinates));
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Object-space vertex format: 'v [x],[y],[z]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Vertex coordinate must be a valid floating-point value."
                        , e);
                    }

                    break;

                case "t":
                    try
                    {
                        string[] TextureCoordinates = Comma().Split(Segments[1]);
                        TextureList.Add(StringArrayToVec2(TextureCoordinates));
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Texture vertex format: 't [u],[v]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Texture coordinate must be a valid floating-point value."
                        , e);
                    }

                    break;

                case "f":
                    try
                    {
                        MatchCollection IndexMatches = CaptureIndices().Matches(Segments[1]);

                        IndexedFace Face = new(
                            Convert.ToInt32(IndexMatches[0].Groups[1].Value),
                            Convert.ToInt32(IndexMatches[1].Groups[1].Value),
                            Convert.ToInt32(IndexMatches[2].Groups[1].Value),
                            
                            Convert.ToInt32(IndexMatches[0].Groups[2].Value),
                            Convert.ToInt32(IndexMatches[1].Groups[2].Value),
                            Convert.ToInt32(IndexMatches[2].Groups[2].Value)
                        );

                        FaceList.Add(Face);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Indexed face format: 'f [v index]/[t index],[v index]/[t index],[v index]/[t index]'."
                        , e);
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            ErrorDetail + "Vertex and texture indices must be valid integer values."
                        , e);
                    }

                    break;
            }
        }

        return new Mesh(VertexList.ToArray(), TextureList.ToArray(), FaceList.ToArray());
    }

	static Vector2 StringArrayToVec2(string[] Arr) => new(
		Convert.ToSingle(Arr[0]),
		Convert.ToSingle(Arr[1])
	);

	static Vector3 StringArrayToVec3(string[] Arr) => new(
		Convert.ToSingle(Arr[0]),
		Convert.ToSingle(Arr[1]),
		Convert.ToSingle(Arr[2])
	);

	[GeneratedRegex(@"\s")]
	private static partial Regex Space();

	[GeneratedRegex(",")]
	private static partial Regex Comma();

	[GeneratedRegex("([0-9]+)/([0-9]+)")]
	private static partial Regex CaptureIndices();
}
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;
using System.Reflection;

namespace ProgrammingClass
{
    internal class ObjResourceReader
    {
        //Normalokkal ellátott obj fájlok olvasásához
        public static unsafe GlObject CreateFromObjFileWithNormals(GL Gl, string filePath, float[] faceColor)
        {
            using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(filePath);

            using var reader = new StreamReader(stream);
            ReadObjWithNormals(reader, out var objVertices, out var objNormals, out var objFaces);

            List<float> glVertices = new();
            List<float> glColors = new();
            List<uint> glIndices = new();
            Dictionary<string, int> glVertexIndices = new();

            foreach (var face in objFaces)
            {
                foreach (var (vIdx, vnIdx) in face)
                {
                    var vertex = objVertices[vIdx - 1];
                    float[] normal;

                    if (vnIdx > 0)
                        normal = objNormals[vnIdx - 1];
                    else
                    {
                        Console.WriteLine("Warning: No normal found for vertex {0}. Using default normal.", vIdx);
                        normal = new float[] { 0, 0, 1 };
                    }

                    var key = $"{vertex[0]} {vertex[1]} {vertex[2]} {normal[0]} {normal[1]} {normal[2]}";

                    if (!glVertexIndices.TryGetValue(key, out int index))
                    {
                        glVertices.AddRange(vertex);
                        glVertices.AddRange(normal);
                        glColors.AddRange(faceColor);
                        index = glVertexIndices[key] = glVertexIndices.Count;
                    }

                    glIndices.Add((uint)index);
                }
            }

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }


        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {
                var aObjVertex = objVertices[objFace[0] - 1];
                var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                var bObjVertex = objVertices[objFace[1] - 1];
                var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                var cObjVertex = objVertices[objFace[2] - 1];
                var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                var normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);
                    glVertex.Add(normal.X);
                    glVertex.Add(normal.Y);
                    glVertex.Add(normal.Z);
                    // add textrure, color

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }
        // Normalokkal ellátott obj fájlok olvasásához
       
        private static void ReadObjWithNormals(TextReader reader, out List<float[]> objVertices, out List<float[]> objNormals, out List<(int v, int vn)[]> objFaces)
        {
            objVertices = new List<float[]>();
            objNormals = new List<float[]>();
            objFaces = new List<(int v, int vn)[]>();

            const float scale = 0.1f;

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                switch (parts[0])
                {
                    case "v":
                        float x = float.Parse(parts[1], CultureInfo.InvariantCulture) * scale;
                        float y = float.Parse(parts[2], CultureInfo.InvariantCulture) * scale;
                        float z = float.Parse(parts[3], CultureInfo.InvariantCulture) * scale;

                        objVertices.Add(new float[]
                        {
                            x,
                            z,
                            y
                        });
                        break;

                    case "vn":
                        objNormals.Add(new float[]
                        {
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                        });
                        break;

                    case "f":
                        var face = new (int v, int vn)[3];
                        for (int i = 0; i < 3; i++)
                        {
                            var segments = parts[i + 1].Split('/');
                            int vertexIndex = int.Parse(segments[0]);
                            int normalIndex = segments.Length >= 3 ? int.Parse(segments[2]) : -1;
                            face[i] = (vertexIndex, normalIndex);
                        }
                        objFaces.Add(face);
                        break;
                }
            }
        }

        private static unsafe void ReadObj(string resourceName, out List<float[]> objVertices, out List<int[]> objFaces)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream(resourceName))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts[0] == "v")
                    {
                        objVertices.Add(new float[]
                        {
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                        });
                    }
                    else if (parts[0] == "f")
                    {
                        objFaces.Add(new int[]
                        {
                            int.Parse(parts[1].Split('/')[0]),
                            int.Parse(parts[2].Split('/')[0]),
                            int.Parse(parts[3].Split('/')[0])
                        });
                    }
                }
            }
        }

     
    }
}

using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;
using System.Globalization;
using System.Reflection;

namespace ProgrammingClass;

internal class ObjResourceReader
{
    public static unsafe GlObject CreateFromObjFileWithNormals(GL gl, string filePath, float[] defaultColor, string mtlPath, float scale = 1.0f)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
        using var reader = new StreamReader(stream);

        ReadObjWithMaterials(scale,reader, mtlPath,
            out var objVertices,
            out var objNormals,
            out var objFaces,
            out var objFaceMaterials,
            out var materials,
            out var materialTextures,
            out var objTexCoords
            );

        List<float> glVertices = new();
        List<float> glColors = new();
        List<uint> glIndices = new();
        Dictionary<string, int> glVertexIndices = new();

        int faceIndex = 0;
        foreach (var face in objFaces)
        {
            string materialName = objFaceMaterials[faceIndex++];
            float[] color = materials.TryGetValue(materialName, out var matColor) ? matColor : defaultColor;

            foreach (var (vIdx, vtIdx, vnIdx) in face)
            {
                var vertex = objVertices[vIdx];
                var normal = vnIdx >= 0 ? objNormals[vnIdx] : new float[] { 0, 0, 1 };
                var tex = vtIdx >= 0 ? objTexCoords[vtIdx] : new float[] { 0f, 0f };

                string key = $"{string.Join(' ', vertex)} {string.Join(' ', normal)} {string.Join(' ', tex)}";
                if (!glVertexIndices.TryGetValue(key, out int index))
                {
                    glVertices.AddRange(vertex); // 3
                    glVertices.AddRange(normal); // 3
                    glVertices.AddRange(tex);    // 2
                    glColors.AddRange(color);
                    index = glVertexIndices[key] = glVertexIndices.Count;
                }

                glIndices.Add((uint)index); 
            }
        }
        uint texId = 0;
        foreach (var mat in objFaceMaterials)
        {
            if (materialTextures.TryGetValue(mat, out var path) && File.Exists(path))
            {
                texId = LoadTextureFromFile(gl, path);
                break;
            }
        }

        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        return CreateOpenGlObject(gl, vao, glVertices, glColors, glIndices, texId);
    }

    private static unsafe GlObject CreateOpenGlObject(GL gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices, uint textureId)
    {
        uint offsetPos = 0;
        uint offsetNormal = offsetPos + 3 * sizeof(float);
        uint offsetTex = offsetNormal + 3 * sizeof(float);
        uint vertexSize = offsetTex + 2 * sizeof(float);

        uint vbo = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, glVertices.ToArray(), GLEnum.StaticDraw);

        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);  // position
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal); // normal
        gl.EnableVertexAttribArray(2);

        gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTex); // texcoord
        gl.EnableVertexAttribArray(3);

        uint cbo = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, cbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, glColors.ToArray(), GLEnum.StaticDraw);

        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
        gl.EnableVertexAttribArray(1);

        uint ebo = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, glIndices.ToArray(), GLEnum.StaticDraw);

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        return new GlObject(vao, vbo, cbo, ebo, (uint)glIndices.Count, gl, textureId);
    }

    private static void ReadObjWithMaterials(
        float scale,
        TextReader reader,
        string mtlFileName,
        out List<float[]> objVertices,
        out List<float[]> objNormals,
        out List<(int v, int vt, int vn)[]> objFaces,
        out List<string> objFaceMaterials,
        out Dictionary<string, float[]> materials,
        out Dictionary<string, string> materialTextures,
        out List<float[]> objTexCoords
        )

    {
        objVertices = new();
        objNormals = new();
        objFaces = new();
        objFaceMaterials = new();
        materials = new();
        materialTextures = new();
        objTexCoords = new();
        

        using var mtlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mtlFileName);
        using var mtlReader = new StreamReader(mtlStream);

        string? currentMtl = null;
        string? mtlLine;

        while ((mtlLine = mtlReader.ReadLine()) != null)
        {
            var parts = mtlLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "newmtl":
                    currentMtl = parts[1];
                    break;
                case "Kd":
                    if (currentMtl != null)
                    {
                        materials[currentMtl] = new float[]
                        {
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture),
                            1.0f
                        };
                    }
                    break;
                case "map_Kd":
                    if (currentMtl != null)
                        materialTextures[currentMtl] = parts[1];
                    break;
            }
        }

        string? line;
        string? currentMaterial = null;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            switch (parts[0])
            {
                case "v":
                    objVertices.Add(new float[]
                    {
                        float.Parse(parts[1], CultureInfo.InvariantCulture) * scale,
                        float.Parse(parts[3], CultureInfo.InvariantCulture) * scale,
                        float.Parse(parts[2], CultureInfo.InvariantCulture) * scale
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

                case "usemtl":
                    currentMaterial = parts[1];
                    break;

                case "f":
                    var faceVerts = parts.Skip(1).Select(p =>
                    {
                        var segs = p.Split('/');
                        int vIdx = int.Parse(segs[0]) - 1;
                        int vtIdx = segs.Length > 1 && segs[1] != "" ? int.Parse(segs[1]) - 1 : -1;
                        int vnIdx = segs.Length > 2 && segs[2] != "" ? int.Parse(segs[2]) - 1 : -1;
                        return (vIdx, vtIdx, vnIdx);
                    }).ToList();

                    for (int i = 1; i < faceVerts.Count - 1; i++)
                    {
                        objFaces.Add(new[] { faceVerts[0], faceVerts[i], faceVerts[i + 1] });
                        objFaceMaterials.Add(currentMaterial ?? "default");
                    }
                    break;
                case "vt":
                    objTexCoords.Add(new float[]
                    {
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        1.0f - float.Parse(parts[2], CultureInfo.InvariantCulture) // Y-t meg kell fordítani OpenGL-hez
                    });
                    break;
            }
        }
    }

    private static unsafe uint LoadTextureFromFile(GL gl, string filePath)
    {
        var image = ImageResult.FromMemory(File.ReadAllBytes(filePath), ColorComponents.RedGreenBlueAlpha);

        uint texture = gl.GenTexture();
        gl.BindTexture(GLEnum.Texture2D, texture);

        fixed (byte* data = image.Data)
        {
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, (uint)image.Width, (uint)image.Height, 0,
                          GLEnum.Rgba, GLEnum.UnsignedByte, data);
        }

        gl.GenerateMipmap(GLEnum.Texture2D);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

        return texture;
    }
}

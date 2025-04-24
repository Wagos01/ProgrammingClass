    using Silk.NET.Maths;
    using Silk.NET.OpenGL;

namespace ProgrammingClass
{

    internal class Collider
    {
        public Vector3D<float> Position { get; set; }
        public float RotationY { get; set; } = 0f;

        public Vector3D<float> Size { get; set; }
        public static bool Visible { get; set; } = true;

        private GL gl;

        private uint vaoSolid, vboSolid;
        private uint vaoWire, vboWire;

        public enum RenderMode
        {
            Wireframe,
            Solid
        }
        public static RenderMode Mode { get; set; } = RenderMode.Wireframe;

        public Collider(Vector3D<float> position, Vector3D<float> size, ref GL gl)
        {
            this.gl = gl;
             Position = position;
            Size = size;

            InitializeBuffers();
        }

        public void Update(float dtime, Vector3D<float> pos, float rotationY = 0f)
        {
            Position = pos;
            RotationY = rotationY;

            if (Visible)
                Draw();


            // Console.WriteLine("Collider: " + Position.X + " " + Position.Y + " " + Position.Z);
        }
       

        public void Draw()
        {
            if (Visible)
            {

                Program.SetModelMatrix(Matrix4X4<float>.Identity);

                if (Mode == RenderMode.Wireframe)
                {
                    float[] wireVertices = GenWireframeVertices();
                    gl.BindVertexArray(vaoWire);
                    gl.BindBuffer(GLEnum.ArrayBuffer, vboWire);
                    gl.BufferData<float>(GLEnum.ArrayBuffer, wireVertices, GLEnum.DynamicDraw);
                    gl.DrawArrays(GLEnum.Lines, 0, (uint)(wireVertices.Length / 3));
                }
                else if (Mode == RenderMode.Solid)
                {
                    float[] solidVertices = GenSolidVertices();
                    gl.BindVertexArray(vaoSolid);
                    gl.BindBuffer(GLEnum.ArrayBuffer, vboSolid);
                    gl.BufferData<float>(GLEnum.ArrayBuffer, solidVertices, GLEnum.DynamicDraw);
                    gl.DrawArrays(GLEnum.Triangles, 0, (uint)(solidVertices.Length / 3));
                }
            }

        }

        public bool Intersects(Collider other)
        {
            return (Math.Abs(Position.X - other.Position.X) * 2 < (Size.X + other.Size.X)) &&
                   (Math.Abs(Position.Y - other.Position.Y) * 2 < (Size.Y + other.Size.Y)) &&
                   (Math.Abs(Position.Z - other.Position.Z) * 2 < (Size.Z + other.Size.Z));
        }

        private float[] GenCubeCorners()
        {
            float halfX = Size.X / 2f;
            float halfY = Size.Y / 2f;
            float halfZ = Size.Z / 2f;

            Vector3D<float>[] localCorners = new Vector3D<float>[]
            {
                new(-halfX, -halfY, -halfZ),
                new( halfX, -halfY, -halfZ),
                new( halfX,  halfY, -halfZ),
                new(-halfX,  halfY, -halfZ),
                new(-halfX, -halfY,  halfZ),
                new( halfX, -halfY,  halfZ),
                new( halfX,  halfY,  halfZ),
                new(-halfX,  halfY,  halfZ)
            };

            var model = Matrix4X4.CreateRotationY(RotationY) * Matrix4X4.CreateTranslation(Position);

            float[] flat = new float[8 * 3];
            for (int i = 0; i < 8; i++)
            {
                var transformed = Vector3D.Transform(localCorners[i], model);
                flat[i * 3 + 0] = transformed.X;
                flat[i * 3 + 1] = transformed.Y;
                flat[i * 3 + 2] = transformed.Z;
            }

            return flat;
        }


        private float[] GenSolidVertices()
        {
            float[] c = GenCubeCorners();
            int[] indices = new int[]
            {
                // Front face
                0, 1, 2,  0, 2, 3,
                // Right face
                1, 5, 6,  1, 6, 2,
                // Back face
                5, 4, 7,  5, 7, 6,
                // Left face
                4, 0, 3,  4, 3, 7,
                // Top face
                3, 2, 6,  3, 6, 7,
                // Bottom face
                4, 5, 1,  4, 1, 0
            };

            float[] solidVertices = new float[indices.Length * 3];
            for (int i = 0; i < indices.Length; i++)
            {
                solidVertices[i * 3 + 0] = c[indices[i] * 3 + 0];
                solidVertices[i * 3 + 1] = c[indices[i] * 3 + 1];
                solidVertices[i * 3 + 2] = c[indices[i] * 3 + 2];
            }

            return solidVertices;

        }

        private float[] GenWireframeVertices()
        {
            float[] c = GenCubeCorners();

            int[] lines = new int[]
            {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };

            float[] wireVertices = new float[lines.Length * 3];
            for (int i = 0; i < lines.Length; i++)
            {
                wireVertices[i * 3 + 0] = c[lines[i] * 3 + 0];
                wireVertices[i * 3 + 1] = c[lines[i] * 3 + 1];
                wireVertices[i * 3 + 2] = c[lines[i] * 3 + 2];
            }

            return wireVertices;
        }

        private unsafe void InitializeBuffers()
        {
            vaoSolid = gl.GenVertexArray();
            vboSolid = gl.GenBuffer();
            gl.BindVertexArray(vaoSolid);
            gl.BindBuffer(GLEnum.ArrayBuffer, vboSolid);
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(36 * 3 * sizeof(float)), null, GLEnum.DynamicDraw);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);

            vaoWire = gl.GenVertexArray();
            vboWire = gl.GenBuffer();
            gl.BindVertexArray(vaoWire);
            gl.BindBuffer(GLEnum.ArrayBuffer, vboWire);
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(24 * 3 * sizeof(float)), null, GLEnum.DynamicDraw);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);

            gl.BindVertexArray(0);
        }
    }


}

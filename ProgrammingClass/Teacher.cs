using Silk.NET.OpenGL;
using System;
using Silk.NET.Maths;

namespace ProgrammingClass
{
    internal class Teacher
    {
        private GlObject body;
        private GlObject rightLeg;
        private GlObject leftLeg;
        private float x = 0f;
        private float y = 0.2f;
        private float z = 5f;

        private float rotationAngle = 0f;
        private float rightLegRotation = 0f;
        private float leftLegRotation = 0f;


        private float animationTime = 0f;
        private float animationSpeed = MathF.PI * 2f;
        private float animationAmplitude = MathF.PI / 16f;


        public Collider collider { get; private set; } = null;

        public Vector3D<float> Position => new(x, y, z);

        public Teacher(ref GL Gl, float scale = 0.015f)
        {
            float[] gray = new float[] { 0.5f, 0.5f, 0.5f };
            body = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.Body.obj", gray, "ProgrammingClass.Resources.Body.mtl", scale);
            rightLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.rightLeg.obj", gray, "ProgrammingClass.Resources.rightLeg.mtl", scale);
            leftLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.leftLeg.obj", gray, "ProgrammingClass.Resources.leftLeg.mtl", scale);

            // Collider méretet a karakter mérete alapján kell beállítani (pl. 1x2x1 doboz)
            collider = new Collider(new Vector3D<float>(0, 0, 0), new Vector3D<float>(0.5f, 2.8f, 0.5f), ref Gl);
        }

        public unsafe void DrawTeacher(ref GL Gl)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);

            Gl.BindTexture(TextureTarget.Texture2D, body.TextureId);
            Program.SetUniformInt("uTexture", 0);
            Program.SetUniformInt("uUseTexture", 1);

            var modelMatrix = Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);
            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(body.Vao);
            Gl.DrawElements(GLEnum.Triangles, body.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, rightLeg.TextureId);
            modelMatrix = Matrix4X4.CreateRotationX(rightLegRotation) * Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);
            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(rightLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, rightLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, leftLeg.TextureId);
            modelMatrix = Matrix4X4.CreateRotationX(leftLegRotation) * Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);
            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(leftLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, leftLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);



            collider.Update(0f, new Vector3D<float>(x + 0, y + 1.2f, z - 0.3f), rotationAngle);
            //collider.Update(0f, new Vector3D<float>(x+0,y+  1.2f,z -0.3f), rotationAngle);
            
            
           // Console.WriteLine("Teacher: " + x + " " + y + " " + z);
        }

        public void Rotate(float radians)
        {
            rotationAngle += radians;
        }

        public void UpdateAnimation(float dtime)
        {
            animationTime += dtime;

            rightLegRotation = MathF.Sin(animationTime * animationSpeed) * animationAmplitude;
            leftLegRotation = MathF.Cos(animationTime * animationSpeed) * animationAmplitude;
        }
        public void test()
        {
            x += 0.1f;
        }

        public void ReleaseGlObject()
        {
            body.ReleaseGlObject();
            rightLeg.ReleaseGlObject();
            leftLeg.ReleaseGlObject();
        }
    }
}

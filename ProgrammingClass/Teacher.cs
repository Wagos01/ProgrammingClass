using Silk.NET.OpenGL;
using System;
using Silk.NET.Maths;
using System.Numerics;

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

        public float rotationAngle { get; private set; } = 0f;
        private float rightLegRotation = 0f;
        private float leftLegRotation = 0f;


        private float animationTime = 0f;
        private float animationSpeed = MathF.PI * 2f;
        private float animationAmplitude = MathF.PI / 12f;
        private float speed = 1.0f;
        private Vector3D<float> StudentPos;
        private float WaitTime = 0f;


        public bool IsWatching { get; set; } = false;

        public Collider collider { get; private set; } = null;

        public Vector3D<float> Position => new(x, y, z);

        public Teacher(ref GL Gl, Vector3D<float> StudentPos, float scale = 0.015f)
        {
            float[] gray = new float[] { 0.5f, 0.5f, 0.5f };
            body = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.Body.obj", gray, "ProgrammingClass.Resources.Body.mtl", scale);
            rightLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.rightLeg.obj", gray, "ProgrammingClass.Resources.rightLeg.mtl", scale);
            leftLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.leftLeg.obj", gray, "ProgrammingClass.Resources.leftLeg.mtl", scale);

            this.StudentPos = StudentPos;

            collider = new Collider(new Vector3D<float>(x,y,z), new Vector3D<float>(0.5f, 2.8f, 0.5f), ref Gl);
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


            var offset = new Vector3(0.06f, 1.2f, 0f);


            Gl.BindTexture(TextureTarget.Texture2D, rightLeg.TextureId);
            //modelMatrix = Matrix4X4.CreateRotationX(rightLegRotation) * Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);
            modelMatrix =
               Matrix4X4.CreateTranslation(-offset.X, -offset.Y, -offset.Z) *
               Matrix4X4.CreateRotationX(rightLegRotation) *
               Matrix4X4.CreateTranslation(offset.X, offset.Y, offset.Z) *
               Matrix4X4.CreateRotationY(rotationAngle) *
               Matrix4X4.CreateTranslation(x, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(rightLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, rightLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, leftLeg.TextureId);

            //modelMatrix = Matrix4X4.CreateRotationX(leftLegRotation) * Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);
            modelMatrix =
               Matrix4X4.CreateTranslation(-offset.X, -offset.Y, -offset.Z) *
               Matrix4X4.CreateRotationX(leftLegRotation) *
               Matrix4X4.CreateTranslation(offset.X, offset.Y, offset.Z) *
               Matrix4X4.CreateRotationY(rotationAngle) *
               Matrix4X4.CreateTranslation(x, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(leftLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, leftLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);



            //collider.Update(0f, new Vector3D<float>(x + 0, y + 1.2f, z - 0.3f), rotationAngle);
            var relativeOffset = new Vector3D<float>(0f, 1.2f, -0.3f);
            var rotatedOffset = Vector3D.Transform(relativeOffset, Matrix4X4.CreateRotationY(rotationAngle));
            var newPos = new Vector3D<float>(x, y, z) + rotatedOffset;
            collider.Update(0f, newPos, rotationAngle);


            // Console.WriteLine("Teacher: " + x + " " + y + " " + z);
        } 

        public void Rotate(float radians)
        {
            rotationAngle += radians;
        }

        public void Update(float dtime)
        {
            if (IsWatching)
            {

                WatchingStudent(dtime);
            }
            else
            {
                Random random = new Random();

                int rand = random.Next(0, 200);
                if (rand == 7)
                {
                    WaitTime = 0;
                    IsWatching = true;
                }
            }

            if (!IsWatching)
            {
                animationTime += dtime;

                rightLegRotation = MathF.Sin(animationTime * animationSpeed) * animationAmplitude;
                leftLegRotation = MathF.Cos(animationTime * animationSpeed) * animationAmplitude;

                Move(dtime);

            }
        }
        private void WatchingStudent(float dtime)
        {
            WaitTime += dtime;
            var direction = Vector3D.Normalize(StudentPos - Position);
            rotationAngle = MathF.Atan2(-direction.X, -direction.Z); 

            if (WaitTime > 2f)
            {
                IsWatching = false;
                SetNewRotation();
            }
        }
        public void SetNewRotation()
        {
            Random rng = new Random();

            float angleChange = (float)(rng.NextDouble() * MathF.PI + MathF.PI / 2); // 0->180 + 90 = 90->270
            if (rng.Next(2) == 0) angleChange *= -1;
            Rotate(angleChange);
            
        }

        public void Move(float dTime)
        {
            x -= MathF.Sin(rotationAngle) * speed * dTime;
            z -= MathF.Cos(rotationAngle) * speed * dTime;
        }

        public void ReleaseGlObject()
        {
            body.ReleaseGlObject();
            rightLeg.ReleaseGlObject();
            leftLeg.ReleaseGlObject();
        }
    }
}

using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using System.Numerics;


namespace ProgrammingClass
{
    internal class Student
    {
        private GlObject body;
        private GlObject rightHand;
        private GlObject leftHand;
        private float x = -4.2f;
        private float y = .1f;
        private float z = 2.8f;
        private float time = 0f;
        public int Lives { get; set; } = 3;

        private float rotationAngle = 0f;
        private float rightHandRotation = 0f;
        private float leftHandRotation = 0f;

        public Vector3D<float> Position => new(x, y, z);

        public bool IsPlaying { get; set; } = false;
        public float score { get; set; } = 0;
        public float highscore { get; set; } = 0;


        public Student(ref GL Gl, float scale = 0.3f)
        {
            float[] gray = new float[] { 0.5f, 0.5f, 0.5f };
            body = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.StudentBody.obj", gray, "ProgrammingClass.Resources.StudentBody.mtl", scale);
            rightHand = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.StudentRightHand.obj", gray, "ProgrammingClass.Resources.StudentRightHand.mtl", scale);
            leftHand = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.StudentLeftHand.obj", gray, "ProgrammingClass.Resources.StudentLeftHand.mtl", scale);
        }
        public unsafe void DrawStudent(ref GL Gl)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, body.TextureId);

            Program.SetUniformInt("uTexture", 0);          // 0 = GL_TEXTURE0
            Program.SetUniformInt("uUseTexture", 1);
            var modelMatrix = Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(x, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(body.Vao);
            Gl.DrawElements(GLEnum.Triangles, body.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);


            Gl.BindTexture(TextureTarget.Texture2D, rightHand.TextureId);

            Program.SetUniformInt("uTexture", 0);          // 0 = GL_TEXTURE0
            Program.SetUniformInt("uUseTexture", 1);
                
            var shoulderOffset = new Vector3(0.06f, 0.6f, 0f);

            modelMatrix =
            Matrix4X4.CreateTranslation(-shoulderOffset.X,-shoulderOffset.Y,-shoulderOffset.Z) *
            Matrix4X4.CreateRotationY(rightHandRotation) *
            Matrix4X4.CreateTranslation(shoulderOffset.X,shoulderOffset.Y, shoulderOffset.Z) *
            Matrix4X4.CreateTranslation(x, y, z);

           // modelMatrix = Matrix4X4.CreateRotationX(rightHandRotation) * Matrix4X4.CreateTranslation(x, y, z);


            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(rightHand.Vao);
            Gl.DrawElements(GLEnum.Triangles, rightHand.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, leftHand.TextureId);


            modelMatrix =
           Matrix4X4.CreateTranslation(-shoulderOffset.X, -shoulderOffset.Y, -shoulderOffset.Z) *
           Matrix4X4.CreateRotationY(leftHandRotation) *
           Matrix4X4.CreateTranslation(shoulderOffset.X, shoulderOffset.Y, shoulderOffset.Z) *
           Matrix4X4.CreateTranslation(x, y, z);

          //  modelMatrix = Matrix4X4.CreateRotationZ(leftHandRotation) * Matrix4X4.CreateTranslation(x, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(leftHand.Vao);
            Gl.DrawElements(GLEnum.Triangles, leftHand.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }


        public void Update(float dtime)
        {
            if (IsPlaying)
            {
                UpdateAnimation(dtime);
            }
           
        }
        private void UpdateAnimation(float dtime)
        {
            time += dtime;

            float speed = MathF.PI * 4; 
            float amplitude = MathF.PI / 32;

            rightHandRotation = MathF.Sin(time * speed) * amplitude;
            leftHandRotation = MathF.Cos(time * speed) * amplitude;
        }
 
        public void ReleaseGlObject()
        {
            body.ReleaseGlObject();
            rightHand.ReleaseGlObject();
            leftHand.ReleaseGlObject();
        }
    }
}

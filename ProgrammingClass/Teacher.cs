using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;


namespace ProgrammingClass
{
    internal class Teacher
    {
        private GlObject body;
        private GlObject rightLeg;
        private GlObject leftLeg;
        private float y = .2f;
        private float z = 5f;

        private float rotationAngle = 0f;
        private float rightLegRotation = 0f;
        private float leftLegRotation = 0f;


        public Teacher(ref GL Gl, float scale = 0.015f)
        {
            float[] gray = new float[] { 0.5f, 0.5f, 0.5f };
            body = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.Body.obj", gray, "ProgrammingClass.Resources.Body.mtl", scale);
            rightLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.rightLeg.obj", gray, "ProgrammingClass.Resources.rightLeg.mtl", scale);
            leftLeg = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.leftLeg.obj", gray, "ProgrammingClass.Resources.leftLeg.mtl", scale);
        }
        public unsafe void DrawTeacher(ref GL Gl)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, body.TextureId);

            Program.SetUniformInt("uTexture", 0);          // 0 = GL_TEXTURE0
            Program.SetUniformInt("uUseTexture", 1);
            var modelMatrix = Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(0f, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(body.Vao);
            Gl.DrawElements(GLEnum.Triangles, body.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);


            Gl.BindTexture(TextureTarget.Texture2D, rightLeg.TextureId);

            Program.SetUniformInt("uTexture", 0);          // 0 = GL_TEXTURE0
            Program.SetUniformInt("uUseTexture", 1);

            modelMatrix = Matrix4X4.CreateRotationX(rightLegRotation)* Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(0f, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(rightLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, rightLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            Gl.BindTexture(TextureTarget.Texture2D, leftLeg.TextureId);
            modelMatrix = Matrix4X4.CreateRotationX(leftLegRotation) * Matrix4X4.CreateRotationY(rotationAngle) * Matrix4X4.CreateTranslation(0f, y, z);

            Program.SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(leftLeg.Vao);
            Gl.DrawElements(GLEnum.Triangles, leftLeg.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }
        public void Rotate(float radians)
        {
            rotationAngle += radians;
        }
        public void RotateRightLeg(float radians)
        {
            rightLegRotation += radians;
        }
        public void RotateLeftLeg(float radians)
        {
            leftLegRotation += radians;
        }
        public void ReleaseGlObject()
        {
            body.ReleaseGlObject();
            rightLeg.ReleaseGlObject();
            leftLeg.ReleaseGlObject();
        }
    }
}

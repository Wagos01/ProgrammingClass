using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System;
using System.Dynamic;
using System.Numerics;
using System.Reflection;
using static ProgrammingClass.CameraDescriptor;
using static System.Formats.Asn1.AsnWriter;

namespace ProgrammingClass
{
    internal class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static ImGuiController imGuiController;

        private static CameraDescriptor camera = new CameraDescriptor();

        private static float timeToRelease = 0;


        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
        private const string ViewPositionVariableName = "uViewPos";

        private const string ShinenessVariableName = "uShininess";

        private static float shininess = 50;

        private static GlObject classRoom;

        private static GlObject table;

        private static Teacher teacher;

        private static Student student;

        private static uint program;

        private static Collider[] colliders = new Collider[4];

        private static bool isGameOver = false;

        private static bool wasCaught = false;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            //cube.Dispose();
            Gl.DeleteProgram(program);
            classRoom.ReleaseGlObject();
            table.ReleaseGlObject();
            teacher.ReleaseGlObject();
            student.ReleaseGlObject();
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            // Handle resizes
            graphicWindow.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };

   


            imGuiController = new ImGuiController(Gl, graphicWindow, inputContext);


            Gl.ClearColor(System.Drawing.Color.White);
            
           // Gl.Enable(EnableCap.CullFace);
           // Gl.CullFace(TriangleFace.Back);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            LinkProgram();

            SetUpObjects();


        }
        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, GetEmbeddedResourceAsString("Shaders.VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, GetEmbeddedResourceAsString("Shaders.FragmentShader.frag"));
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);

            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {

            }

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
        }
        private static string GetEmbeddedResourceAsString(string resourceRelativePath)
        {
            string resourceFullPath = Assembly.GetExecutingAssembly().GetName().Name + "." + resourceRelativePath;

            using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFullPath))
            using (var resStreamReader = new StreamReader(resStream))
            {
                var text = resStreamReader.ReadToEnd();
                return text;
            }
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.U:
                    camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Space:
                    student.IsPlaying = true;
                    break;
               
            }
        }
        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Space:
                    student.IsPlaying = false;
                    break;
            }
        }


        private static void GraphicWindow_Update(double deltaTime)
        {
           if(student.Lives <= 0)
            {
                isGameOver = true;
            }
           

            imGuiController.Update((float)deltaTime);
            student.Update((float)deltaTime);

            TeacherLogic((float)deltaTime);

            if(!teacher.IsWatching && student.IsPlaying && !isGameOver)
            {
                student.score += (float)deltaTime;
                if (student.score > student.highscore)
                {
                    student.highscore = student.score;
                }
            }
        }

        private static void TeacherLogic(float deltaTime)
        {
            teacher.Update((float)deltaTime);

            foreach (var c in colliders)
            {
                if (c.Intersects(teacher.collider))
                {
                    teacher.SetNewRotation();
                }
            }

            if(teacher.IsWatching)
            {
                timeToRelease += deltaTime;

                if(timeToRelease > 0.5f && student.IsPlaying && !wasCaught)
                {
                    student.Lives--;
                    wasCaught = true;
                }
            }
            else
            {
                timeToRelease = 0;
                wasCaught = false;
            }
        }
        public static void RestartGame()
        {
            student.Lives = 3;
            student.IsPlaying = false;
            teacher.IsWatching = false;
            timeToRelease = 0;
            isGameOver = false;
            student.score = 0;
            wasCaught = false;
        }




        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetUniform3(LightColorVariableName, new Vector3(1f, 1f, 1f));
            SetUniform3(LightPositionVariableName, new Vector3(0f, 10f, 0f));
            SetUniform3(ViewPositionVariableName, new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUniform1(ShinenessVariableName, shininess);

            Matrix4X4<float> viewMatrix;

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            switch (CameraDescriptor.currentView)
            {
                case CameraDescriptor.CameraViewMode.Default:
                    viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
                    break;

                case CameraDescriptor.CameraViewMode.StudentView:
                    viewMatrix = Matrix4X4.CreateLookAt(
                        camera.GetStudentCameraPosition(student.Position),
                        camera.GetStudentCameraTarget(teacher.Position),
                        Vector3D<float>.UnitY
                    );
                    break;

                case CameraDescriptor.CameraViewMode.TeacherFollow:
                    viewMatrix = Matrix4X4.CreateLookAt(
                        camera.GetTeacherFollowCameraPosition(teacher.Position, teacher.rotationAngle),
                        camera.GetTeacherFollowCameraTarget(teacher.Position),
                        Vector3D<float>.UnitY
                    );
                    break;
                default:
                    viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
                    break;

            }
            SetMatrix(viewMatrix, ViewMatrixVariableName);


            SetUniformInt("uUseTexture", 0);// nincs textura
            DrawClassRoom();
            DrawTable();


            foreach(var c in colliders)
            {
                c.Draw();
            }

         
            teacher.DrawTeacher(ref Gl);
            student.DrawStudent(ref Gl);

            ImGuiNET.ImGui.Begin("Game Info|Settings", ImGuiNET.ImGuiWindowFlags.AlwaysAutoResize | ImGuiNET.ImGuiWindowFlags.NoCollapse);
           

            bool colliderVisible = Collider.Visible; 
            ImGuiNET.ImGui.Checkbox("ColliderVisible", ref colliderVisible); 
            Collider.Visible = colliderVisible;
            
            
            if (ImGui.BeginCombo("ColliderType", Collider.Mode.ToString()))
            {
                foreach (Collider.RenderMode value in Enum.GetValues(typeof(Collider.RenderMode)))
                {
                    bool isSelected = Collider.Mode == value;
                    if (ImGui.Selectable(value.ToString(), isSelected))
                    {
                        Collider.Mode = value;
                    }
                }
                ImGui.EndCombo();
            }



            if (ImGui.BeginCombo("CameraView", CameraDescriptor.currentView.ToString()))
            {
                foreach (CameraDescriptor.CameraViewMode value in Enum.GetValues(typeof(CameraDescriptor.CameraViewMode)))
                {
                    bool isSelected = CameraDescriptor.currentView == value;
                    if (ImGui.Selectable(value.ToString(), isSelected))
                    {
                        CameraDescriptor.currentView = value;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Text($"Score: {student.score}");
            ImGui.Text($"HighScore: {student.highscore}");
            ImGui.Text($"Lives: {student.Lives}");

            if (isGameOver)
            {
                ImGui.Begin("Game Over", ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text($"Score: {student.score}");
                ImGui.Text($"HighScore: {student.highscore}");

                if (ImGui.Button("Restart"))
                {
                    RestartGame();
                }
                ImGui.End();
            }

            ImGuiNET.ImGui.End();

            imGuiController.Render();
        }
        private static unsafe void DrawTable()
        {
            var modelMatrixForTable = Matrix4X4.CreateScale(1f, 1f, 1f);
            SetModelMatrix(modelMatrixForTable);
            Gl.BindVertexArray(table.Vao);
            Gl.DrawElements(GLEnum.Triangles, table.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }
        private static unsafe void DrawClassRoom()
        {
            var modelMatrix = Matrix4X4.CreateTranslation(0f, 0f, 5f);
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(classRoom.Vao);
            Gl.DrawElements(GLEnum.Triangles, classRoom.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }
        private static unsafe void SetUpObjects()
        {

            float[] gray = [0.5f, 0.5f, 0.5f, 1.0f];




            classRoom = ObjResourceReader.CreateFromObjFileWithNormals(Gl, "ProgrammingClass.Resources.ClassRoom.obj", gray, "ProgrammingClass.Resources.ClassRoom.mtl");
            student = new Student(ref Gl);
            teacher = new Teacher(ref Gl, student.Position);

            float[] tableColor = [System.Drawing.Color.Azure.R/256f,
                                  System.Drawing.Color.Azure.G/256f,
                                  System.Drawing.Color.Azure.B/256f,
                                  1f];
            table = GlCube.CreateSquare(Gl, tableColor);

            Collider collider = new Collider(new Vector3D<float>(0, 2, 0), new Vector3D<float>(10f, 3f, 2.5f), ref Gl);
            Collider collider1 = new Collider(new Vector3D<float>(0, 2, 8.7f), new Vector3D<float>(10f, 3f, 2.5f), ref Gl);
            Collider collider2 = new Collider(new Vector3D<float>(3.5f, 2, 4f), new Vector3D<float>(2.5f, 3f, 10f), ref Gl);
            Collider collider3 = new Collider(new Vector3D<float>(-5f, 2, 4f), new Vector3D<float>(2.5f, 3f, 10f), ref Gl);
            colliders[0] = collider;
            colliders[1] = collider1;
            colliders[2] = collider2;
            colliders[3] = collider3;
        }

        public static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            SetMatrix(modelMatrix, ModelMatrixVariableName);

            // set also the normal matrix
            int location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }

            // G = (M^-1)^T
            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));

            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUniform1(string uniformName, float uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform1(location, uniformValue);
            CheckError();
        }

        private static unsafe void SetUniform3(string uniformName, Vector3 uniformValue)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform3(location, uniformValue);
            CheckError();
        }
        public static unsafe void SetUniformInt(string uniformName, int value)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.Uniform1(location, value);
            CheckError();
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}
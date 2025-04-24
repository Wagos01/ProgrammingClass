
using Silk.NET.Maths;

namespace ProgrammingClass
{
    internal class CameraDescriptor
    {
        public double DistanceToOrigin { get; private set; } = 14;

        public double AngleToZYPlane { get; private set; } = 0;

        public double AngleToZXPlane { get; private set; } = 1;

        const double DistanceScaleFactor = 1.1;

        const double AngleChangeStepSize = Math.PI / 180 * 5;

        public enum CameraViewMode
        {
            Default,
            StudentView,
            TeacherFollow
        }
        public static CameraViewMode currentView = CameraViewMode.StudentView;


        /// <summary>
        /// Gets the position of the camera.
        /// </summary>
        public Vector3D<float> Position
        {
            get
            {
                return GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane);
            }
        }

        /// <summary>
        /// Gets the up vector of the camera.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return Vector3D.Normalize(GetPointFromAngles(DistanceToOrigin, AngleToZYPlane, AngleToZXPlane + Math.PI / 2));
            }
        }

        /// <summary>
        /// Gets the target point of the camera view.
        /// </summary>
        public Vector3D<float> Target
        {
            get
            {
                // For the moment the camera is always pointed at the origin.
                return Vector3D<float>.Zero;
            }
        }

        public void IncreaseZXAngle()
        {
            AngleToZXPlane += AngleChangeStepSize;
        }

        public void DecreaseZXAngle()
        {
            AngleToZXPlane -= AngleChangeStepSize;
        }

        public void IncreaseZYAngle()
        {
            AngleToZYPlane += AngleChangeStepSize;

        }

        public void DecreaseZYAngle()
        {
            AngleToZYPlane -= AngleChangeStepSize;
        }

        public void IncreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin * DistanceScaleFactor;
        }

        public void DecreaseDistance()
        {
            DistanceToOrigin = DistanceToOrigin / DistanceScaleFactor;
        }

        private static Vector3D<float> GetPointFromAngles(double distanceToOrigin, double angleToMinZYPlane, double angleToMinZXPlane)
        {
            var x = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Sin(angleToMinZYPlane);
            var z = distanceToOrigin * Math.Cos(angleToMinZXPlane) * Math.Cos(angleToMinZYPlane);
            var y = distanceToOrigin * Math.Sin(angleToMinZXPlane);

            return new Vector3D<float>((float)x, (float)y, (float)z);
        }

        public void SetStudentView(Vector3D<float> studentPos, Vector3D<float> teacherPos)
        {
            DistanceToOrigin = 1;
            AngleToZXPlane = 0;
            AngleToZYPlane = 0;
        }

        public Vector3D<float> GetStudentCameraPosition(Vector3D<float> studentPos)
        {
            return studentPos + new Vector3D<float>(-2f, 3f, 0f);
        }

        public Vector3D<float> GetStudentCameraTarget(Vector3D<float> teacherPos)
        {
            return teacherPos;
        }


        public Vector3D<float> GetTeacherFollowCameraPosition(Vector3D<float> teacherPos, float rotationAngle)
        {
            const float distance = 3f;
            const float heightOffset = 3f;

            var offset = new Vector3D<float>(
                MathF.Sin(rotationAngle) * distance,
                heightOffset,
                MathF.Cos(rotationAngle) * distance
            );

            return teacherPos + offset;
        }


        public Vector3D<float> GetTeacherFollowCameraTarget(Vector3D<float> teacherPos)
        {
            return teacherPos + new Vector3D<float>(0, 1.5f, 0);
        }

    }
}

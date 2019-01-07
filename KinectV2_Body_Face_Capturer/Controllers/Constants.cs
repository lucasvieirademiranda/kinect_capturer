using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace KinectV2_Fingerspelling.Controllers
{

    /// <summary>
    /// Skeleton Joint Upper Type
    /// </summary>
    public enum JointUpType
    {
        SpineBase = 0,
        SpineMid = 1,
        Neck = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        HipRight = 13,
        SpineShoulder = 14,
        HandTipLeft = 15,
        ThumbLeft = 16,
        HandTipRight = 17,
        ThumbRight = 18,
    }

    /// <summary>
    /// Visualization modes for the streams provided by the sensor
    /// </summary>
    public enum VisTypes
    {
        /// <summary>
        /// No visualizaton
        /// </summary>
        None = 0,

        /// <summary>
        /// Color mode (1920x1080)
        /// </summary>
        Color = 1,

        /// <summary>
        /// Depth mode (512x424)
        /// </summary>
        Depth = 2,

        /// <summary>
        /// BodyIndex mode  (512x424)
        /// </summary>
        BodyIndex = 3,

        /// <summary>
        /// Infrared mode (512x424)
        /// </summary>
        Infrared = 4,

    }

    /// <summary>
    /// Visualization size of the color space
    /// </summary>
    public struct ColorImageSize
    {
        /// <summary>
        /// space width
        /// </summary>
        public int Width;

        /// <summary>
        /// space height
        /// </summary>
        public int Height;

    }

    /// <summary>
    /// Visualization size of the camera view size
    /// </summary>
    public struct DepthImageSize
    {
        /// <summary>
        /// space width
        /// </summary>
        public int Width;

        /// <summary>
        /// space height
        /// </summary>
        public int Height;

    }

    /// <summary>
    /// Storage for face features
    /// </summary>
    public struct FaceData
    {
        /// <summary>
        /// Bounding box in color space
        /// </summary>
        public BoxFace boxColor;

        /// <summary>
        /// Bounding box in depth space
        /// </summary>
        public BoxFace boxDepth;

        /// <summary>
        /// Face data constructor
        /// </summary>
        /// <param name="v1">vector4.</param>
        /// <param name="v2">vector4.</param>
        public FaceData(BoxFace v1, BoxFace v2)
        {
            this.boxColor = v1;
            this.boxDepth = v2;
        }

    }

    /// <summary>
    /// Bounding box [X, Y, Width, Height]
    /// </summary>
    public struct BoxFace
    {
        public int topX;
        public int topY;
        public int width;
        public int height;

        public BoxFace(int val1, int val2, int val3, int val4)
        {
            this.topX = val1;
            this.topY = val2;
            this.width = val3;
            this.height = val4;
        }
        public void Print()
        {
            Console.WriteLine("Box ({0},{1}) ({2},{3})", topX, topY, width, height);
        }

    }

    /// <summary>
    /// The skeleton of a single body
    /// </summary>
    public struct SkeletonOfBody
    {
        public string[] jointName;
        public System.Windows.Point[] jointColorSpace;
        public System.Windows.Point[] jointDepthSpace;
        public CameraSpacePoint[] jointCameraSpace;
        public Vector4[] jointQuaternion;

        // Preload default values over the skeleton
        public SkeletonOfBody(int numJoints)
        {
            jointName = new string[numJoints];
            jointColorSpace = new System.Windows.Point[numJoints];
            jointDepthSpace = new System.Windows.Point[numJoints];
            jointCameraSpace = new CameraSpacePoint[numJoints];
            jointQuaternion = new Vector4[numJoints];

            // Load zero to joint positions
            for (int i = 0; i < numJoints; i++)
            {
                //jointName[i] = " ";

                jointColorSpace[i].X = 0;
                jointColorSpace[i].Y = 0;

                jointDepthSpace[i].X = 0;
                jointDepthSpace[i].Y = 0;

                jointCameraSpace[i].X = 0;
                jointCameraSpace[i].Y = 0;
                jointCameraSpace[i].Z = 0;

                jointQuaternion[i].W = 0;
                jointQuaternion[i].X = 0;
                jointQuaternion[i].Y = 0;
                jointQuaternion[i].Z = 0;
            }

            // Load the default joint names
            if (numJoints == Constants.SKEL_TOTAL_JOINTS)
            {
                jointName = Constants.SKEL_JOINT_NAMES;
            }
            else
            {
                jointName = Constants.SKEL_UP_JOINT_NAMES;
            }
            //Console.WriteLine("Skeleton Instance");
        }

    }

    /// <summary>
    /// Provides constants to process the Kinect frames
    /// </summary>
    public static class Constants
    {
        // This value depends of the type of sensor
        public static readonly int NUM_BODIES = 6;
        public static readonly int COLOR_BYTES_PER_PIXEL = 4;
        public static readonly int MIN_DEPTH = 500;
        public static readonly int MAX_DEPTH = 4500;

        public static readonly int SKEL_TOTAL_JOINTS = 25;
        public static readonly string[] SKEL_JOINT_NAMES = new string[SKEL_TOTAL_JOINTS];

        public static readonly int[] SKEL_UP_INDEX = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 16, 20, 21, 22, 23, 24 };
        public static readonly int SKEL_UP_TOTAL_JOINTS = SKEL_UP_INDEX.Length;
        public static readonly string[] SKEL_UP_JOINT_NAMES = new string[SKEL_UP_TOTAL_JOINTS];

        public static readonly List<Tuple<JointType, JointType>> BONES = null;
        public static readonly List<Tuple<JointType, JointType>> BONES_UP = null;


        /// <summary>
        ///  Constructor
        /// </summary>
        static Constants()
        {

            // Load an array with the Joints Names provided by Kinect
            for (int ii = 0; ii < SKEL_TOTAL_JOINTS; ii++)
            {
                SKEL_JOINT_NAMES[ii] = ((JointType)ii).ToString();

                //Console.WriteLine("Joint_Name: " + ((JointType)i).ToString());                
                // SpineBase = 0
                // SpineMid = 1
                // Neck = 2
                // Head = 3
                // ShoulderLeft = 4
                // ElbowLeft = 5
                // WristLeft = 6
                // HandLeft = 7
                // ShoulderRight = 8
                // ElbowRight = 9
                // WristRight = 10
                // HandRight = 11
                // HipLeft = 12
                // KneeLeft = 13
                // AnkleLeft = 14
                // FootLeft = 15
                // HipRight = 16
                // KneeRight = 17
                // AnkleRight = 18
                // FootRight = 19
                // SpineShoulder = 20
                // HandTipLeft = 21
                // ThumbLeft = 22
                // HandTipRight = 23
                // ThumbRight = 24
            }

            // Load an array with the Joints Names of the upper skeleton provided by Kinect
            int jj = 0;
            foreach (var pos in SKEL_UP_INDEX)
            {
                SKEL_UP_JOINT_NAMES[jj] = ((JointType)pos).ToString();
                jj++;
                // 0    SpineBase = 0
                // 1    SpineMid = 1,
                // 2    Neck = 2
                // 3    Head = 3,
                // 4    ShoulderLeft = 4
                // 5    ElbowLeft = 5
                // 6    WristLeft = 6
                // 7    HandLeft = 7
                // 8    ShoulderRight = 8
                // 9    ElbowRight = 9
                //10    WristRight = 10
                //11    HandRight = 11
                //12    HipLeft = 12
                //13    HipRight = 16
                //14    SpineShoulder = 20
                //15    HandTipLeft = 21
                //16    ThumbLeft = 22
                //17    HandTipRight = 23
                //18    ThumbRight = 24
            }


            // A bone defined as a line between two joints
            // We establish bones as connections between joints
            BONES_UP = new List<Tuple<JointType, JointType>>();

            // Torso
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            BONES_UP.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // A bone defined as a line between two joints
            BONES = new List<Tuple<JointType, JointType>>();

            // Torso
            BONES.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            BONES.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            BONES.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            BONES.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            BONES.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            BONES.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            BONES.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            BONES.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));


        }

    }

}

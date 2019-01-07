
#define FACE_DETECTION  // Enable/disable face feature detection

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using KinectV2_Fingerspelling.Extensions;


namespace KinectV2_Fingerspelling.Controllers
{
    /// <summary>
    /// Interaction logic to run, and stop the depth sensor
    /// </summary>
    public class DepthSensorKinect2
    {
        /// <summary>
        /// The kinect sensor
        /// </summary>
        private KinectSensor sensor = null;

        /// <summary>
        /// Frame handler
        /// </summary>
        private Action<byte[], ushort[], byte[], byte[], IList<Body>, FaceData> frameHandler;


        /// <summary>
        /// Number of frames to capture from the sensor
        /// </summary>
        private int framesToCapture = 1;

        /// <summary>
        /// A global frame count
        /// </summary>
        private long frameCount = 0;

        /// <summary>
        /// Captured frames in the recording
        /// </summary>
        private int recordedFrames = 0;

        /// <summary>
        /// Message of the sensor
        /// </summary>
        string statusText = "";

        // Coordinate mapper to map one type of point to another        
        private CoordinateMapper coordinateMapper = null;

        // Multisource frame reader
        private MultiSourceFrameReader allFrameReader = null;

        // Description of the data contained in the color frame
        private FrameDescription colorFrameDescription = null;

        // Description of the data contained in the depth frame
        private FrameDescription depthFrameDescription = null;

        // Description of the data contained in the body index frame        
        private FrameDescription bodyIndexFrameDescription = null;

        // Intermediate storage for color frame data 
        private byte[] buffColor32 = null;

        // Intermediate storage for depth frame data 
        private ushort[] buffDepth16 = null;

        // Intermediate storage for depth frame data multiplied
        //private ushort[] buffDepth16Copy = null;

        // Intermediate storage for body index frame data converted to color        
        private byte[] buffBodyIndex8 = null;

        // Intermediate storage of the current skeleton data
        private IList<Body> listBodies = null;

        // Intermediate storage for colored depth frame
        private byte[] buffMapDepthToColor32 = null;

        // Intermediate storage of the colored depth points
        private ColorSpacePoint[] buffColorSpacePoints = null;

        // Intermediate storage of the current face data
        private FaceData faceData = new FaceData(new BoxFace(0, 0, 0, 0), new BoxFace(0, 0, 0, 0));

        // Space of visualization
        private ColorImageSize colorImageSize = new ColorImageSize();
        private DepthImageSize depthImageSize = new DepthImageSize();

        // Fix Recording Frames
        private int fixedFrames = -1;

        // Frame rate variables
        private int frameRate = -1;

        private DateTime lastTime = System.DateTime.MaxValue;

        /// <summary>
        /// The sensor state of recording
        /// </summary>
        private bool stateOfRecording = false;

        /// <summary>
        /// data container object
        /// </summary>
        private DataContainer dataContainer = null;

        /// <summary>
        /// Draw markups over the color image
        /// </summary>
        private bool drawingDepthMarkups = false;

#if FACE_DETECTION
        /// <summary>
        /// The face frame source
        /// </summary>
        FaceFrameSource faceFrameSource = null;

        /// <summary>
        /// The face frame reader
        /// </summary>
        FaceFrameReader faceFrameReader = null;

        /// <summary>
        /// Storage for face frame results
        /// </summary>
        private FaceFrameResult faceFrameResults = null;

#endif

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sensor">physical sensor.</param>
        /// <param name="fsColor">color frame source.</param>
        /// <param name="fsDepth">depth frame source.</param>
        /// <param name="fsBodyIndex">body index frame source.</param>
        /// <param name="fsBody">body frame source.</param>
        public DepthSensorKinect2(KinectSensor sensor)
        {
            // Get the default sensor
            this.sensor = sensor;

            // Open the sensor
            this.sensor.Open();

            // Get the coordinate mapper of the sensor
            this.coordinateMapper = this.sensor.CoordinateMapper;

            // Get FrameDescription from ColorFrameSource
            this.colorFrameDescription = this.sensor.ColorFrameSource.FrameDescription;

            // Get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.sensor.DepthFrameSource.FrameDescription;

            // Get the body index frame information
            this.bodyIndexFrameDescription = this.sensor.BodyIndexFrameSource.FrameDescription;

            // Initialize the view space parameters
            this.colorImageSize.Width = this.colorFrameDescription.Width;
            this.colorImageSize.Height = this.colorFrameDescription.Height;
            this.depthImageSize.Width = this.depthFrameDescription.Width;
            this.depthImageSize.Height = this.depthFrameDescription.Height;

            // Initialize a list to save the body skeleton 
            this.listBodies = new Body[this.sensor.BodyFrameSource.BodyCount]; //this.kinectSensor.BodyFrameSource.BodyCount = 6

            // Allocation of space to store the raw data to be received
            this.buffColor32 = new byte[this.colorFrameDescription.Width * this.colorFrameDescription.Height * 4];
            this.buffDepth16 = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
            //this.buffDepth16Copy = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
            this.buffBodyIndex8 = new byte[this.bodyIndexFrameDescription.Width * this.bodyIndexFrameDescription.Height];

            this.buffMapDepthToColor32 = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height * 4];
            this.buffColorSpacePoints = new ColorSpacePoint[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // Setup the multiframe reader
            this.allFrameReader = this.sensor.OpenMultiSourceFrameReader(
                                                FrameSourceTypes.Color // color frame
                                                | FrameSourceTypes.Depth  // depth frame                                                                        
                                                | FrameSourceTypes.BodyIndex // body index frame
                                                | FrameSourceTypes.Body // body 
                                                );
            this.allFrameReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

#if FACE_DETECTION
            // Setup the face source with the desired features
            this.faceFrameSource = new FaceFrameSource(this.sensor, 0,
                FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.BoundingBoxInInfraredSpace
            );

            // Setup the face reader
            this.faceFrameReader = this.faceFrameSource.OpenReader();
            this.faceFrameReader.FrameArrived += FaceReader_FrameArrived;
#endif

            // Zeroing variables and counters
            this.recordedFrames = 0;
            this.frameCount = 0;
            this.stateOfRecording = false;
            this.dataContainer = new DataContainer();
            this.ResetFPS();

            // Update sensor status text
            this.statusText = "Kinect Ready! ";
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {

            // All frame counter
            this.frameCount++;
            if (this.frameCount % this.framesToCapture != 0) return;

            ColorFrame colorFrame = null;
            DepthFrame depthFrame = null;
            BodyFrame bodyFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            Body body = null;
            SkeletonOfBody skel_up = new SkeletonOfBody(Constants.SKEL_UP_TOTAL_JOINTS);

            try
            {
                var frameReference = e.FrameReference.AcquireFrame();

                colorFrame = frameReference.ColorFrameReference.AcquireFrame();
                depthFrame = frameReference.DepthFrameReference.AcquireFrame();
                bodyFrame = frameReference.BodyFrameReference.AcquireFrame();
                bodyIndexFrame = frameReference.BodyIndexFrameReference.AcquireFrame();

                if (colorFrame == null || depthFrame == null || bodyFrame == null || bodyIndexFrame == null)
                {
                    return;
                }

                //--------------------------------------------
                // Get the color frame
                //--------------------------------------------
                using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                {
                    colorFrame.CopyConvertedFrameDataToArray(this.buffColor32, ColorImageFormat.Bgra);
                } //End ColorFrame

                //--------------------------------------------
                // Get the depth frame
                //--------------------------------------------
                using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                {
                    depthFrame.CopyFrameDataToArray(this.buffDepth16);
                    //depthFrame.CopyFrameDataToArray(this.buffDepth16Copy);

                    // Multiplication by 20 only to turn the depth visually more perceptible
                    //int i = 0;
                    //Array.ForEach(this.buffDepth16Copy, (x) => { this.buffDepth16Copy[i++] = (ushort)(x * 20); });

                } //End DepthFrame

                //--------------------------------------------
                // Get the body index frame
                //--------------------------------------------
                using (KinectBuffer bodyIndexBuffer = bodyIndexFrame.LockImageBuffer())
                {
                    bodyIndexFrame.CopyFrameDataToArray(this.buffBodyIndex8);
                }

                //--------------------------------------------
                // Get the body frame
                //--------------------------------------------
                bodyFrame.GetAndRefreshBodyData(this.listBodies);
                //bodyFrame.FloorClipPlane.

                //--------------------------------------------
                // Map the depth frame to it color frame
                //--------------------------------------------                
                {
                    Array.Clear(this.buffColorSpacePoints, 0, this.buffColorSpacePoints.Length);
                    Array.Clear(this.buffMapDepthToColor32, 0, this.buffMapDepthToColor32.Length);

                    // Coordinate mapping
                    this.coordinateMapper.MapDepthFrameToColorSpace(this.buffDepth16, this.buffColorSpacePoints);

                    unsafe
                    {
                        fixed (ColorSpacePoint* depthMappedToColorPointsPointer = buffColorSpacePoints)
                        {
                            // Loop over each row and column of the color image
                            // Zero out any pixels that don't correspond to a body index
                            for (int idxDepth = 0; idxDepth < buffColorSpacePoints.Length; ++idxDepth)
                            {
                                float depthMappedToColorX = depthMappedToColorPointsPointer[idxDepth].X;
                                float depthMappedToColorY = depthMappedToColorPointsPointer[idxDepth].Y;

                                // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                                if (!float.IsNegativeInfinity(depthMappedToColorX) &&
                                    !float.IsNegativeInfinity(depthMappedToColorY))
                                {
                                    // Make sure the depth pixel maps to a valid point in color space
                                    int colorX = (int)(depthMappedToColorX + 0.5f);
                                    int colorY = (int)(depthMappedToColorY + 0.5f);

                                    // If the point is not valid, there is no body index there.
                                    if ((colorX >= 0) && (colorX < this.colorImageSize.Width) && (colorY >= 0) && (colorY < this.colorImageSize.Height))
                                    {
                                        int idxColor = (colorY * this.colorImageSize.Width) + colorX;

                                        // If we are tracking a body for the current pixel, save the depth data
                                        if (this.buffBodyIndex8[idxDepth] != 0xff)
                                        {
                                            this.buffMapDepthToColor32[idxDepth * 4] = this.buffColor32[idxColor * 4]; // B
                                            this.buffMapDepthToColor32[idxDepth * 4 + 1] = this.buffColor32[idxColor * 4 + 1]; // G
                                            this.buffMapDepthToColor32[idxDepth * 4 + 2] = this.buffColor32[idxColor * 4 + 2]; // R
                                        }
                                    }
                                }
                            }
                        }
                    } //End Unsafe

                } //End Mapping


                //--------------------------------------------
                // Process the face of the default body
                //--------------------------------------------

                // Variable to save the detected face paramenters
                this.faceData = new FaceData(new BoxFace(0, 0, 0, 0), new BoxFace(0, 0, 0, 0));

#if FACE_DETECTION

               
                // Get the default body
                // Body body = this.listBodies.Where(b => b.IsTracked).FirstOrDefault();
                if (this.faceFrameSource.IsActive)
                {
                    // In our experiment we get the closest body                        
                    body = Util.GetClosestBody(this.listBodies);

                    if (body != null && body.IsTracked)
                    {
                        // Get the first skeleton
                        skel_up = Util.GetSkeletonUpperBody(this.Mapper, body);

                        // Draw skeleton joints                        
                        if (this.drawingDepthMarkups)
                        {
                            Util.WriteSkeletonOverFrame(this, VisTypes.Depth, skel_up, 2, ref this.buffMapDepthToColor32);
                            //Util.WriteSkeletonOverFrame(this, VisTypes.Depth, skeleton, 2, ref this.buffDepth16);
                        }

                        // Assign a tracking ID to the face source
                        this.faceFrameSource.TrackingId = body.TrackingId;

                        if (this.faceFrameResults != null)
                        {
                            var boxColor = this.faceFrameResults.FaceBoundingBoxInColorSpace;
                            var boxDepth = this.faceFrameResults.FaceBoundingBoxInInfraredSpace;

                            // If there are face results, then save data
                            // We save in a format of rectangle [x, y, width, height]
                            this.faceData.boxColor = new BoxFace(boxColor.Left, boxColor.Top, (boxColor.Right - boxColor.Left), (boxColor.Bottom - boxColor.Top));
                            this.faceData.boxDepth = new BoxFace(boxDepth.Left, boxDepth.Top, (boxDepth.Right - boxDepth.Left), (boxDepth.Bottom - boxDepth.Top));

                            // Draw the face
                            if (this.drawingDepthMarkups)
                            {
                                Util.WriteFaceOverFrame(this, VisTypes.Depth, faceData.boxDepth, 1, ref this.buffMapDepthToColor32);
                                //Util.WriteFaceOverFrame(this, VisTypes.Depth, faceData.boxDepth, 1, ref this.buffDepth16);                                
                            }//End Drawing

                        }//End FaceResult

                    } //End Body
                }
#endif

                // Update the data handler 
                this.frameHandler(
                    this.buffColor32,
                    this.buffDepth16,
                    this.buffBodyIndex8,
                    this.buffMapDepthToColor32,
                    this.listBodies,
                    this.faceData
                    );

                // Recording state ture
                byte[] _colorData = null;
                ushort[] _depthData = null;
                byte[] _bodyIndexData = null;
                IList<Body> _bodies = null;

                //-------------------------------------------- 
                // Record the data
                //--------------------------------------------

                if (this.stateOfRecording)
                {
                    // 25-09-15
                    // Discard frames where the hand is not corrected tracked (i.e., the hand has a zero coordinate)
                    // To discard failures in hand tracking
                    if (skel_up.jointDepthSpace[(int)JointUpType.HandLeft].X == 0 || skel_up.jointDepthSpace[(int)JointUpType.HandLeft].Y == 0
                       || skel_up.jointDepthSpace[(int)JointUpType.HandRight].X == 0 || skel_up.jointDepthSpace[(int)JointUpType.HandRight].Y == 0)
                    {
                        Console.WriteLine("Neglect frame {0}", this.recordedFrames);
                        return;
                    }

                    // Storage data;
                    _colorData = new byte[this.buffColor32.Length];
                    _depthData = new ushort[this.buffDepth16.Length];
                    _bodyIndexData = new byte[this.buffBodyIndex8.Length];
                    _bodies = new Body[this.listBodies.Count];

                    colorFrame.CopyConvertedFrameDataToArray(_colorData, ColorImageFormat.Bgra);
                    depthFrame.CopyFrameDataToArray(_depthData);
                    bodyIndexFrame.CopyFrameDataToArray(_bodyIndexData);
                    bodyFrame.GetAndRefreshBodyData(_bodies);

                    // Increase the counter
                    this.recordedFrames++;

                    this.dataContainer.AddColor = _colorData;
                    this.dataContainer.AddDepth = _depthData;
                    this.dataContainer.AddBodyIndex = _bodyIndexData;
                    this.dataContainer.AddListOfBodies = _bodies;
                    this.dataContainer.AddFaceData = this.faceData;


                    // If the user only require to save a fixed number of frames
                    if (this.fixedFrames == this.recordedFrames)
                    {
                        this.stateOfRecording = false;
                    }

                }


                // Notice:
                // Array.Copy() --> how many elements to copy
                // Buffer.BlockCopy --> how many of bytes to copy    

                // Update Frame Rate
                UpdateGrabberFrameRate();

            }
            finally
            {
                if (this.frameCount > 100000000)
                    this.frameCount = 0;
                if (colorFrame != null)
                    colorFrame.Dispose();
                if (depthFrame != null)
                    depthFrame.Dispose();
                if (bodyFrame != null)
                    bodyFrame.Dispose();
                if (bodyIndexFrame != null)
                    bodyIndexFrame.Dispose();
            }
        }


#if FACE_DETECTION
        private void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            FaceFrame faceFrame = null;
            try
            {
                faceFrame = e.FrameReference.AcquireFrame();

                if (faceFrame == null)
                {
                    return;
                }

                // Copy the face result
                this.faceFrameResults = faceFrame.FaceFrameResult;

            }
            finally
            {
                if (faceFrame != null)
                    faceFrame.Dispose();
            }
        }
#endif

        /// <summary>
        /// Get the view color space size
        /// </summary>
        public ColorImageSize ColorImageSize
        {
            get { return this.colorImageSize; }
        }


        /// <summary>
        /// Get the view depth space size
        /// </summary>
        public DepthImageSize DepthImageSize
        {
            get { return this.depthImageSize; }
        }

        public bool GetBuffers(Action<byte[], ushort[], byte[], byte[], IList<Body>, FaceData> fHandler)
        {
            /// Color mode (1920x1080)        
            /// Depth mode (512x424)
            /// Infrared mode (512x424)        
            /// BodyIndex mode  (512x424)
            /// Body mode (25 3D joints)

            this.frameHandler = fHandler;

            this.statusText = "Successful buffering setting!";
            return true;
        }

        /// <summary>
        /// Get the sensor
        /// </summary>
        public KinectSensor Sensor
        {
            get { return this.sensor; }
            //set { this.sensor = value; }
        }

        /// <summary>
        /// Get the status text of the sensor
        /// </summary>
        public string StatusText
        {
            get { return this.statusText; }
            //set { this.msgStatus = value; }
        }


        /// <summary>
        /// Reset the frame rate of the grabber
        /// </summary>
        public void ResetFPS()
        {
            this.lastTime = System.DateTime.MaxValue;
            this.grabberTotalFrames = 0;
            this.grabberLastFrames = 0;
        }


        /// <summary>
        /// Number of frames of the grabber
        /// </summary>
        private int grabberTotalFrames { get; set; }
        private int grabberLastFrames { get; set; }

        /// <summary>
        /// Return the sensor CoordinateMapper object
        /// </summary>
        public CoordinateMapper Mapper
        {
            get { return this.coordinateMapper; }
        }


        /// <summary>
        /// The current frame rate
        /// </summary>
        public int FPS
        {
            get { return this.frameRate; }
        }

        private void FrameRate(int value)
        {
            if (this.frameRate != value)
            {
                this.frameRate = value;
            }
        }

        /// <summary>
        /// Total of recorded frames
        /// </summary>
        public int RecordedFrames
        {
            set { this.recordedFrames = value; }
            get { return this.recordedFrames; }
        }

        /// <summary>
        /// Sensor to capture data
        /// </summary>
        public bool IsRecording
        {
            set { this.stateOfRecording = value; }
            get { return this.stateOfRecording; }
        }


        /// <summary>
        /// Update the frame rate of the grabber
        /// </summary>
        private void UpdateGrabberFrameRate()
        {
            ++this.grabberTotalFrames;
            DateTime cur = System.DateTime.Now;
            var span = cur.Subtract(this.lastTime);
            if (this.lastTime == System.DateTime.MaxValue || span >= TimeSpan.FromSeconds(1))
            {
                // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                // rounding yields a more balanced result
                this.FrameRate((int)Math.Round((this.grabberTotalFrames - this.grabberLastFrames) / span.TotalSeconds));
                this.grabberLastFrames = this.grabberTotalFrames;
                this.lastTime = cur;

                // Update frame rate label
                //System.Console.WriteLine("FR {0}", FPS());
            }

        }

        /// <summary>
        /// Number of fixed frames to be recorded
        /// </summary>
        public int FixedFrames
        {
            set { this.fixedFrames = value; }
        }

        /// <summary>
        /// Get the data container structure
        /// </summary>
        public DataContainer Data
        {
            get { return this.dataContainer; }
        }

        /// <summary>
        /// Set the number of frames to capture
        /// </summary>
        public int FramesToCapture
        {
            set
            {
                this.framesToCapture = (int)(30 / value);
                this.frameCount = 0;
            }
        }


        /// <summary>
        /// Set on drawing the markups
        /// </summary>
        public bool DrawingDepthMarkups
        {
            set { this.drawingDepthMarkups = value; }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DepthSensorKinect2()
        {
            // FrameReder is IDisposable
            if (this.allFrameReader != null)
            {
                this.allFrameReader.MultiSourceFrameArrived -= Reader_MultiSourceFrameArrived;
                this.allFrameReader.Dispose();
                this.allFrameReader = null;
            }

#if FACE_DETECTION
            if (this.faceFrameReader != null)
            {
                this.faceFrameReader.FrameArrived -= FaceReader_FrameArrived;
                this.faceFrameReader.Dispose();
                this.faceFrameReader = null;
            }
#endif

            this.sensor = null;
        }

    }
}

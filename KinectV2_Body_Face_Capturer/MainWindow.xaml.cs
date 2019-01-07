
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

// Kinect
using Microsoft.Kinect;

// Emgu CV
using Emgu.CV;
using Emgu.CV.Structure;

// Sub-packages
using KinectV2_Fingerspelling.Extensions;
using KinectV2_Fingerspelling.Controllers;
using KinectV2_Fingerspelling.ShapeProcessing;


namespace KinectV2_Fingerspelling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Similar directories to the filename
        /// </summary>
        private int countDirectories = 0;

        /// <summary>
        /// Path of the current directory
        /// </summary>
        private string mainPath;

        /// <summary>
        /// A depth sensor
        /// </summary>
        private DepthSensorKinect2 depthSensor = null;

        /// <summary>
        /// Time of occurrence of a process/event
        /// </summary>
        private DateTime startTime = System.DateTime.MaxValue;

        // Variables to control FPS
        private int grabberFrameRate = -1;
        private DateTime lastTime = System.DateTime.MaxValue;

        // Pointers to retrieved frame data 
        private byte[] ptrColorImage32 = null;
        private ushort[] ptrDepthImage16 = null;
        private byte[] ptrBodyIndexImage8 = null;
        private IList<Body> ptrBodies = null;
        private FaceData ptrFaceData;
        private byte[] ptrMapDepthToColor32 = null;

        // Temporal buffers to worked data
        private ushort[] buffDepth16Mul = null;
        private byte[] buffDepth8TH = null;

        // BitmapS to images on the GUI
        private WriteableBitmap bitmapOutMapDepthToColor = null;
        private WriteableBitmap bitmapOutDepth = null;
        private WriteableBitmap bitmapOutColorFrame = null;
        

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        ///</summary> 
        public MainWindow()
        {
            // Initialize the components (controls) of the window
            InitializeComponent();
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {

            // Enable (Disable) / Hide some controls
            btnRecordStop.IsEnabled = false;
            lblRecording.Visibility = System.Windows.Visibility.Hidden;
            lblRecordingFrames.Visibility = System.Windows.Visibility.Hidden;
            lblRecordingFramesCount.Visibility = System.Windows.Visibility.Hidden;
            lblRecordingSecs.Visibility = System.Windows.Visibility.Hidden;
            lblRecordingSecsCount.Visibility = System.Windows.Visibility.Hidden;
            this.InitComboFrameToCapture();

            // Save the current path
            this.mainPath = System.IO.Directory.GetCurrentDirectory();

            // KINECT SENSOR
            #region KinectSensor

            // Initialize the kinectSensor object and activate frame stream to be displayed
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.InitializeSensor();
            })); // End Dispatcher

            #endregion

            // Initialize the function frame rate counter
            //this.ResetFunctionFrameRate();                      

        }


        /// <summary>
        /// Initialize a physical sensor
        /// </summary>
        private void InitializeSensor()
        {

            this.depthSensor = new DepthSensorKinect2(KinectSensor.GetDefault());
            this.txtStatusBar.Text = this.depthSensor.StatusText;

            // Create bitmaps
            #region createOutBitmaps

            // Create the bitmap to display depth frames
            this.bitmapOutDepth = new WriteableBitmap(this.depthSensor.DepthImageSize.Width, this.depthSensor.DepthImageSize.Height, 96.0, 96.0, PixelFormats.Gray16, null);
            this.imgOutDepth.Source = this.bitmapOutDepth;

            // Create the bitmap to display the colored depth frames
            this.bitmapOutMapDepthToColor = new WriteableBitmap(this.depthSensor.DepthImageSize.Width, this.depthSensor.DepthImageSize.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.imgOutMapDepthToColor.Source = this.bitmapOutMapDepthToColor;

            // Create the bitmap to display the thresholded depth frames
            this.bitmapOutColorFrame = new WriteableBitmap(this.depthSensor.ColorImageSize.Width, this.depthSensor.ColorImageSize.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
           this.imgOutColorFrame.Source = this.bitmapOutColorFrame;

            #endregion


            #region CreateBuffers

            this.buffDepth16Mul = new ushort[this.depthSensor.DepthImageSize.Width * this.depthSensor.DepthImageSize.Width];
            
            #endregion

            // Link writeable controls to recovery frames
            this.depthSensor.GetBuffers(this.OnFrame);

            // Diplay messages
            #region messages

            //lblColorFrame.Content = "Color Frame " + this.depthSensor.ViewColorSize.colorWidth.ToString() + "x" + this.depthSensor.ViewColorSize.colorHeight.ToString();
            lblDepthFrame.Content = "Depth Frame " + this.depthSensor.DepthImageSize.Width.ToString() + "x" + this.depthSensor.DepthImageSize.Height.ToString();
            lblMapDepthToColorFrame.Content = "Map Depth to Color Frame " + this.depthSensor.DepthImageSize.Width.ToString() + "x" + this.depthSensor.DepthImageSize.Height.ToString();
            this.txtStatusBar.Text = this.depthSensor.StatusText;

            #endregion

        }

        private void OnFrame(byte[] frameColor32, ushort[] frameDepth16, byte[] frameBodyIndex8,
            byte[] frameMapDepthToColor32, IList<Body> listBodies, FaceData faceData)
        {
            // Get pointers to the data in the current enviroment
            this.ptrColorImage32 = frameColor32;
            this.ptrDepthImage16 = frameDepth16;
            this.ptrBodyIndexImage8 = frameBodyIndex8;
            this.ptrMapDepthToColor32 = frameMapDepthToColor32;
            this.ptrBodies = listBodies;
            this.ptrFaceData = faceData;

            // To record data and update information
            if (this.depthSensor.IsRecording == true)
            {
                this.CounterOfFrames();
                this.UpdateCounterOfSeconds();
            }

            // Display frames
            this.DisplayFrames(frameColor32, frameDepth16, frameMapDepthToColor32);

            // Display 2D Skeleton
            //this.DisplayArm(listBodies, frameDepth16); //Elias

            // Display the grabber FPS
            this.lblFrameRate.Content = "Frame Rate: " + this.depthSensor.FPS;

            // Update this Function Frame Rate
            //this.UpdateFunctionFrameRate();
        }


        /// <summary>
        /// Display frames
        /// </summary>
        /// <param name="frameDepth16">gray16</param>
        /// <param name="frameMapDepthToColor">rgb32</param>
        void DisplayFrames(byte[] frameColo32, ushort[] frameDepth16, byte[] frameMapDepthToColor)
        {

            //--------------------------------------------
            // Display the depth frame
            //--------------------------------------------

            DIP.BufferClone(frameDepth16, buffDepth16Mul);
            DIP.BufferMultiplyRange(buffDepth16Mul, 20);

            // Update the bitmap
            Util.UpdateOutBitmap(this.bitmapOutDepth, buffDepth16Mul);

            //--------------------------------------------
            // Display the colored depth frame
            //--------------------------------------------

            // Update the bitmap
            Util.UpdateOutBitmap(this.bitmapOutColorFrame, 4, frameColo32);
            Util.UpdateOutBitmap(this.bitmapOutMapDepthToColor, 4, frameMapDepthToColor);

        }


        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWin_Closing(object sender, CancelEventArgs e)
        {
            GC.Collect();
        }


        protected int grabberTotalFrames { get; set; }

        protected int grabberLastFrames { get; set; }

        /// <summary>
        /// The current frame rate
        /// </summary>
        private int FrameRate
        {
            get
            {
                return this.grabberFrameRate;
            }

            set
            {
                if (this.grabberFrameRate != value)
                {
                    this.grabberFrameRate = value;
                }
            }
        }

        /// <summary>
        /// Update the frame rate of the grabber
        /// </summary>
        private void UpdateFunctionFrameRate()
        {
            ++this.grabberTotalFrames;
            DateTime cur = System.DateTime.Now;
            var span = cur.Subtract(this.lastTime);
            if (this.lastTime == System.DateTime.MaxValue || span >= TimeSpan.FromSeconds(1))
            {
                // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                // rounding yields a more balanced result
                this.FrameRate = (int)Math.Round((this.grabberTotalFrames - this.grabberLastFrames) / span.TotalSeconds);
                this.grabberLastFrames = this.grabberTotalFrames;
                this.lastTime = cur;

                // Update frame rate label
                this.lblFrameRate.Content = "Main Frame Rate: " + this.grabberFrameRate.ToString();
            }
        }


        /// <summary>
        /// Reset the frame rate of the grabber
        /// </summary>
        protected void ResetFunctionFrameRate()
        {
            this.lastTime = System.DateTime.MaxValue;
            this.grabberTotalFrames = 0;
            this.grabberLastFrames = 0;
        }

        /// <summary>
        /// Count the frames after starting the recording event
        /// </summary>
        private void CounterOfFrames()
        {
            this.lblRecordingFramesCount.Content = this.depthSensor.RecordedFrames;
        }

        /// <summary>
        /// Counter of seconds after the start recording event
        /// </summary>
        private void UpdateCounterOfSeconds()
        {
            DateTime cur = System.DateTime.Now;
            TimeSpan span = cur.Subtract(this.startTime);
            this.lblRecordingSecsCount.Content = Math.Round(span.TotalSeconds, 2);
        }


        private void btnRecordStart_Click(object sender, RoutedEventArgs e)
        {

            this.btnRecordStop.IsEnabled = true; 
            this.btnRecordStart.IsEnabled = false;            
            this.txtFilename.IsEnabled = false;
            this.txtFixedFrames.IsEnabled = false;
            this.cmbSetFrameToCapture.IsEnabled = false;

            lblRecording.Visibility = System.Windows.Visibility.Visible;
            lblRecordingFrames.Visibility = System.Windows.Visibility.Visible;
            lblRecordingFramesCount.Visibility = System.Windows.Visibility.Visible;
            lblRecordingSecs.Visibility = System.Windows.Visibility.Visible;
            lblRecordingSecsCount.Visibility = System.Windows.Visibility.Visible;

            // Start the recording            
            this.txtStatusBar.Text = "Capturing frames!";
            this.startTime = System.DateTime.Now;

            // Send message to the sensor      
            this.depthSensor.IsRecording = true;
            this.depthSensor.FixedFrames = int.Parse(txtFixedFrames.Text);
        }

        private void btnRecordStop_Click(object sender, RoutedEventArgs e)
        {
            // Message
            this.txtStatusBar.Text = "Saving frames!";
            this.btnRecordStop.IsEnabled = false;
            this.btnRecordStart.IsEnabled = true;            
            this.txtFilename.IsEnabled = true;
            this.txtFixedFrames.IsEnabled = true;
            this.txtFixedFrames.Text = "-1";
            this.cmbSetFrameToCapture.IsEnabled = true;

            // Stop the recording                   

            // Send message to the sensor    
            this.depthSensor.IsRecording = false;
            this.depthSensor.FixedFrames = int.Parse(txtFixedFrames.Text);

            //lblRecording.Visibility = System.Windows.Visibility.Hidden;
            //lblRecordingFrames.Visibility = System.Windows.Visibility.Hidden;
            //lblRecordingFramesCount.Visibility = System.Windows.Visibility.Hidden;
            //lblRecordingSecs.Visibility = System.Windows.Visibility.Hidden;
            //lblRecordingSecsCount.Visibility = System.Windows.Visibility.Hidden;

            // Write on the disk the captured images
            this.SaveRecordedData();

            // Send message to the sensor
            this.depthSensor.ResetFPS();
            this.depthSensor.Data.ClearData();
            this.depthSensor.RecordedFrames = 0;

           
        }

        private void SaveRecordedData()
        {
            // To prevent overwrite an existing directory, we count the number of START_RECORD event
            this.countDirectories++;

            // Specify a name for your top-level folder.
            string filePath = System.IO.Path.Combine(mainPath, txtFilename.Text);

            // Prevent to overwrite directory container
            if (Directory.Exists(filePath))
            {
                filePath += this.countDirectories;
            }

            // Create a new directory
            System.IO.Directory.CreateDirectory(filePath);
            // Change directory
            System.IO.Directory.SetCurrentDirectory(filePath);

            // Create file patterns
            string colorPattern = txtFilename.Text + "_color_";
            string depthPattern = txtFilename.Text + "_depth_";
            string depthBodyIndexPattern = txtFilename.Text + "_body_";

            string skelPattern = txtFilename.Text + "_skel_";

            string colorWithDepthPattern = txtFilename.Text + "_coldep_";
            string depthWithColorPattern = txtFilename.Text + "_depcol_";

            string faceDataPattern = txtFilename.Text + "_face_";

            //Save collected data            
            //depthSensor.SaveColorFrames(colorPattern); // In color space
            //depthSensor.SaveMapOfColorWithDepth(colorWithDepthPattern); // In color space
            depthSensor.SaveDepthFrames(depthPattern); // In depth space
            depthSensor.SaveMapOfDepthWithColor(depthWithColorPattern); // In depth space            
            //depthSensor.SaveSkeletonFrames(skelPattern); // Complete skeleton in color and depth space
            depthSensor.SaveSkeletonUpFrames(skelPattern); // Upper skeleton in color and depth space
            //depthSensor.SaveFaceData(faceDataPattern); // In both color and depth space
            //depthSensor.SaveBodyIndexFrames(depthBodyIndexPattern); // Since the depthInColor and colorInDepth are based in the body index frame, it is redundant            
            // Return to the main path directory            
            System.IO.Directory.SetCurrentDirectory(mainPath);
        }


        private void btnCaptureOneShot_Click(object sender, RoutedEventArgs e)
        {
            // Change to the main work directory            
            System.IO.Directory.SetCurrentDirectory(mainPath);

            // Create a new directory and change to it
            string time = System.DateTime.Now.ToString("hh'_'mm'_'ss", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
            string filePath = System.IO.Path.Combine("shot_" + time);
            System.IO.Directory.CreateDirectory(filePath);
            System.IO.Directory.SetCurrentDirectory(filePath);

            // Save the color frame to writeable bitmap
            WriteableBitmap picColor = Util.ToWriteableBitmap(this.depthSensor.ColorImageSize, this.ptrColorImage32);
#if GLOBAL_SAVE_COLOR_JPEG            
            Util.SaveImageShotJPEG(picColor, filePath, "color");
#else
            Util.SaveImageShotPNG(picColor, filePath, "color");
#endif

            // Save the depth frame 
            WriteableBitmap picDepth = Util.ToWriteableBitmap(this.depthSensor.DepthImageSize, this.ptrDepthImage16);
            Util.SaveImageShotPNG(picDepth, filePath, "depth");

            // Save the depth frame mapped to color space
            byte[] mapColor32 = Util.MapDepthToColor(this.depthSensor, this.ptrColorImage32,
                this.ptrDepthImage16, this.ptrBodyIndexImage8);
            WriteableBitmap picMapColor = Util.ToWriteableBitmap(this.depthSensor.DepthImageSize, mapColor32);
            //Util.SaveImageShot(picMapColor, filePath, "DepCol");

            // DIRECT FROM WRITEABLE SOURCE
            // Save the color frame mapped in depth space 
            Util.SaveImageShotPNG(this.bitmapOutMapDepthToColor, filePath, "depCol");


            // Save the color frame mapped to depth space
            ushort[] mapDepth16 = Util.MapColorToDepth(this.depthSensor, this.ptrColorImage32,
                        this.ptrDepthImage16, this.ptrBodyIndexImage8);
            WriteableBitmap picMapDepth = Util.ToWriteableBitmap(this.depthSensor.ColorImageSize, mapDepth16);
            Util.SaveImageShotPNG(picMapDepth, filePath, "colDep");

            // Save the skeleton
            //Util.SaveBodyShotJoints(this.ptrBodies, this.depthSensor.Mapper, filePath, "Skel");
            Util.SaveBodyShotUpperJoints(this.ptrBodies, this.depthSensor.Mapper, filePath, "skel");

            // Save the face data
            Util.SaveFaceShot(this.ptrFaceData, filePath, "face");

            // Return to the main work directory            
            System.IO.Directory.SetCurrentDirectory(mainPath);

        }

        private void chkMapJoints_Click(object sender, RoutedEventArgs e)
        {
            this.depthSensor.DrawingDepthMarkups = (bool)this.chkMapJoints.IsChecked;
        }


        /// <summary>
        /// Menu Camera parameter click
        /// </summary>
        /// <param name="sender">Obj.</param>
        /// <param name="e">Arg.</param>
        private void mnuDepthCamera_Click(object sender, RoutedEventArgs e)
        {
            CameraIntrinsics depthInt = this.depthSensor.Mapper.GetDepthCameraIntrinsics();

            string msg = "Focal Lenght:\n" +
                        "\tfx : " + depthInt.FocalLengthX.ToString() + "\n" +
                        "\tfy : " + depthInt.FocalLengthY.ToString() + "\n" +
                        "Principal Point:\n" +
                        "\tcx : " + depthInt.PrincipalPointX.ToString() + "\n" +
                        "\tcy : " + depthInt.PrincipalPointY.ToString() + "\n";
            MessageBox.Show(msg, "Depth Camera Intrinsic Parameter", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        /// <summary>
        /// Load intems to combo box
        /// </summary>
        private void InitComboFrameToCapture()
        {
            List<int> frameToCapture = new List<int> { 30, 15, 10, 5, 1 };
            this.cmbSetFrameToCapture.ItemsSource = frameToCapture;
            this.cmbSetFrameToCapture.SelectedIndex = 0;

        }

        /// <summary>
        /// Selection change of combo to set frame to capture
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">params</param>
        private void cmbSetFrameToCapture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.depthSensor != null)
            {
                this.depthSensor.FramesToCapture = (int) this.cmbSetFrameToCapture.SelectedItem;
                //Console.WriteLine(this.cmbSetFrameToCapture.SelectedItem);
            }
        }


    }
}



using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Kinect;
using KinectV2_Fingerspelling.Controllers;

namespace KinectV2_Fingerspelling.Extensions
{
    public static class DepthSensorKinect2Extensions
    {

        #region Save Frames

        public static void SaveMapOfDepthWithColor(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<byte[]> _listColor = sensor.Data.AllColor;
            List<ushort[]> _listDepth = sensor.Data.AllDepth;
            List<byte[]> _listBodyIndex = sensor.Data.AllBodyIndex;

            int fileCount = 1;
            string filename;

            WriteableBitmap picture = new WriteableBitmap(
                sensor.DepthImageSize.Width,
                sensor.DepthImageSize.Height,
                96.0, 96.0, PixelFormats.Bgr32, null);

            ColorSpacePoint[] depthToColorSpacePoints = new ColorSpacePoint[sensor.DepthImageSize.Width * sensor.DepthImageSize.Height];
            byte[] buffData32 = new byte[sensor.DepthImageSize.Width * sensor.DepthImageSize.Height * 4];

            byte[] colorData = null;
            byte[] bodyIndexData = null;
            ushort[] depthData = null;

            for (int frame = 0; frame < _listDepth.Count; frame++)
            {
                colorData = _listColor[frame];
                depthData = _listDepth[frame];
                bodyIndexData = _listBodyIndex[frame];


                // Coordinate mapping
                sensor.Mapper.MapDepthFrameToColorSpace(depthData, depthToColorSpacePoints);
                Array.Clear(buffData32, 0, buffData32.Length);

                unsafe
                {
                    fixed (ColorSpacePoint* depthMappedToColorPointsPointer = depthToColorSpacePoints)
                    {
                        // Loop over each row and column of the color image
                        // Zero out any pixels that don't correspond to a body index
                        for (int depthIndex = 0; depthIndex < depthToColorSpacePoints.Length; ++depthIndex)
                        {
                            float depthMappedToColorX = depthMappedToColorPointsPointer[depthIndex].X;
                            float depthMappedToColorY = depthMappedToColorPointsPointer[depthIndex].Y;

                            // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                            if (!float.IsNegativeInfinity(depthMappedToColorX) &&
                                !float.IsNegativeInfinity(depthMappedToColorY))
                            {
                                // Make sure the depth pixel maps to a valid point in color space
                                int colorX = (int)(depthMappedToColorX + 0.5f);
                                int colorY = (int)(depthMappedToColorY + 0.5f);

                                // If the point is not valid, there is no body index there.
                                if ((colorX >= 0) && (colorX < sensor.ColorImageSize.Width) && (colorY >= 0) && (colorY < sensor.ColorImageSize.Height))
                                {
                                    int index = (colorY * sensor.ColorImageSize.Width) + colorX;

                                    // If we are tracking a body for the current pixel, save the depth data
                                    if (bodyIndexData[depthIndex] != 0xff)
                                    {
                                        buffData32[depthIndex * 4] = colorData[index * 4]; // B
                                        buffData32[depthIndex * 4 + 1] = colorData[index * 4 + 1]; // G
                                        buffData32[depthIndex * 4 + 2] = colorData[index * 4 + 2]; // R

                                    }
                                }
                            }
                        }
                    }
                }

                // Copy array data into the writeable bitmap
                picture.Lock();
                picture.WritePixels(
                    new Int32Rect(0, 0, sensor.DepthImageSize.Width, sensor.DepthImageSize.Height),
                            buffData32, sensor.DepthImageSize.Width * 4, 0);
                picture.Unlock();

                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderPNG = new PngBitmapEncoder();
                encoderPNG.Frames.Add(BitmapFrame.Create(picture));

                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".png";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderPNG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }

                // Remove the last inserted frame
                encoderPNG.Frames.Clear();

                fileCount++;
            }

            Console.WriteLine("Saved {0} Depth in Color Images", fileCount - 1);

        }//End Method

        public static void SaveMapOfColorWithDepth(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<ushort[]> _listDepth = sensor.Data.AllDepth;
            List<byte[]> _listBodyIndex = sensor.Data.AllBodyIndex;

            int fileCount = 1;
            string filename;

            WriteableBitmap picture = new WriteableBitmap(
                sensor.ColorImageSize.Width,
                sensor.ColorImageSize.Height,
                96.0, 96.0, PixelFormats.Gray16, null);

            DepthSpacePoint[] colorToDepthSpacePoints = new DepthSpacePoint[sensor.ColorImageSize.Width * sensor.ColorImageSize.Height];
            ushort[] buffData16 = new ushort[sensor.ColorImageSize.Width * sensor.ColorImageSize.Height];

            byte[] bodyIndexData = null;
            ushort[] depthData = null;

            for (int frame = 0; frame < _listDepth.Count; frame++)
            {
                depthData = _listDepth[frame];
                bodyIndexData = _listBodyIndex[frame];

                // Coordinate mapping
                sensor.Mapper.MapColorFrameToDepthSpace(depthData, colorToDepthSpacePoints);
                Array.Clear(buffData16, 0, buffData16.Length);

                unsafe
                {
                    fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = colorToDepthSpacePoints)
                    {
                        // Loop over each row and column of the color image
                        // Zero out any pixels that don't correspond to a body index
                        for (int colorIndex = 0; colorIndex < colorToDepthSpacePoints.Length; ++colorIndex)
                        {
                            float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                            float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;

                            // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                            if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                !float.IsNegativeInfinity(colorMappedToDepthY))
                            {
                                // Make sure the depth pixel maps to a valid point in color space
                                int depthX = (int)(colorMappedToDepthX + 0.5f);
                                int depthY = (int)(colorMappedToDepthY + 0.5f);

                                // If the point is not valid, there is no body index there.
                                if ((depthX >= 0) && (depthX < sensor.DepthImageSize.Width) && (depthY >= 0) && (depthY < sensor.DepthImageSize.Height))
                                {
                                    int index = (depthY * sensor.DepthImageSize.Width) + depthX;

                                    // If we are tracking a body for the current pixel, save the depth data
                                    if (bodyIndexData[index] != 0xff)
                                    {
                                        buffData16[colorIndex] = depthData[index];
                                    }
                                }
                            }
                        }
                    }
                }

                // Copy array data into the writeable bitmap
                picture.Lock();
                picture.WritePixels(
                    new Int32Rect(0, 0, sensor.ColorImageSize.Width, sensor.ColorImageSize.Height),
                            buffData16, sensor.ColorImageSize.Width * 2, 0);
                picture.Unlock();

                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderPNG = new PngBitmapEncoder();
                encoderPNG.Frames.Add(BitmapFrame.Create(picture));

                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".png";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderPNG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }

                // Remove the last inserted frame
                encoderPNG.Frames.Clear();

                fileCount++;
            }

            Console.WriteLine("Saved {0} Color in Depth Images", fileCount - 1);

        }//End Method

        public static void SaveBodyIndexFrames(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<byte[]> _listBodyIndex = sensor.Data.AllBodyIndex;

            // Create a temporal bitmap to save depth frames
            WriteableBitmap picture = new WriteableBitmap(
                sensor.DepthImageSize.Width,
                sensor.DepthImageSize.Height,
                96.0, 96.0, PixelFormats.Gray8, null);

            int fileCount = 1;
            string filename;

            foreach (byte[] bodyIndexData in _listBodyIndex)
            {
                // Copy array data into the writeable bitmap
                picture.Lock();
                picture.WritePixels(
                    new Int32Rect(0, 0, sensor.DepthImageSize.Width, sensor.DepthImageSize.Height),
                            bodyIndexData, sensor.DepthImageSize.Width * 1, 0);
                picture.Unlock();

                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderPNG = new PngBitmapEncoder();
                encoderPNG.Frames.Add(BitmapFrame.Create(picture));

                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".png";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderPNG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }

                // Remove the last inserted frame
                encoderPNG.Frames.Clear();

                fileCount++;
            } //EndForEach

            Console.WriteLine("Saved {0} Body Index Images", fileCount - 1);

        }//End Method

        public static void SaveDepthFrames(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<ushort[]> _listDepth = sensor.Data.AllDepth;

            // Create a temporal bitmap to save depth frames
            WriteableBitmap picture = new WriteableBitmap(
                sensor.DepthImageSize.Width,
                sensor.DepthImageSize.Height,
                96.0, 96.0, PixelFormats.Gray16, null);

            int fileCount = 1;
            string filename;

            foreach (ushort[] depthData in _listDepth)
            {
                // Copy array data into the writeable bitmap
                picture.Lock();
                picture.WritePixels(
                    new Int32Rect(0, 0, sensor.DepthImageSize.Width, sensor.DepthImageSize.Height),
                            depthData, sensor.DepthImageSize.Width * 2, 0);
                picture.Unlock();

                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderPNG = new PngBitmapEncoder();
                encoderPNG.Frames.Add(BitmapFrame.Create(picture));

                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".png";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderPNG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }

                // Remove the last inserted frame
                encoderPNG.Frames.Clear();

                fileCount++;
            }

            Console.WriteLine("Saved {0} Depth Images", fileCount - 1);

        }//End Method

        public static void SaveColorFrames(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<byte[]> _listColor = sensor.Data.AllColor;

            // Create a temporal bitmap to save depth frames
            WriteableBitmap picture = new WriteableBitmap(
                sensor.ColorImageSize.Width,
                sensor.ColorImageSize.Height,
                96.0, 96.0, PixelFormats.Bgra32, null);

            int fileCount = 1;
            string filename;

            foreach (byte[] colorData in _listColor)
            {
                // Copy array data into the writeable bitmap
                picture.Lock();
                picture.WritePixels(
                    new Int32Rect(0, 0, sensor.ColorImageSize.Width, sensor.ColorImageSize.Height),
                            colorData, sensor.ColorImageSize.Width * 4, 0);
                picture.Unlock();

#if GLOBAL_SAVE_COLOR_JPEG

                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderJPEG = new JpegBitmapEncoder()  ;                    
                encoderJPEG.Frames.Add(BitmapFrame.Create(picture));
                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".jpeg";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderJPEG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }
                // Remove the last inserted frame
                encoderJPEG.Frames.Clear();

#else
                // Create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoderPNG = new PngBitmapEncoder();
                encoderPNG.Frames.Add(BitmapFrame.Create(picture));
                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".png";

                // Write image
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoderPNG.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write file {0}", filename);
                }
                // Remove the last inserted frame
                encoderPNG.Frames.Clear();
#endif

                fileCount++;
            }

            Console.WriteLine("Saved {0} Color Images", fileCount - 1);

        }//End Method

        #endregion


        #region Save CSV Data

        public static void SaveSkeletonFrames(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<IList<Body>> _listOfBodies = sensor.Data.AllListOfBodies;
            int fileCount = 1;
            string filename;
            SkeletonOfBody skeleton = new SkeletonOfBody(Constants.SKEL_TOTAL_JOINTS);

            // Traverse the skeleton list
            foreach (IList<Body> _bodies in _listOfBodies)
            {
                
                // Get the most closest tracked skeleton
                Body body = Util.GetClosestBody(_bodies);
                //Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                // Process the tracked body
                if (body != null && body.IsTracked)
                {
                    // Get the complete skeleton
                    skeleton = Util.GetSkeletonBody(sensor.Mapper, body);

                } // EndIf

                //-------------------------------------------
                // Write the skeleton joints into a file
                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".csv";
                Util.Skeleton2File(filename, skeleton);

                fileCount++;

            }// EndForEach

            Console.WriteLine("Saved {0} Body Data Files", fileCount - 1);
        } //End Method

        public static void SaveSkeletonUpFrames(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<IList<Body>> _listOfBodies = sensor.Data.AllListOfBodies;
            int fileCount = 1;
            string filename;            
            SkeletonOfBody skeleton = new SkeletonOfBody(Constants.SKEL_UP_TOTAL_JOINTS);

            // Traverse the skeleton list
            foreach (IList<Body> _bodies in _listOfBodies)
            {                
                // Get the most closest tracked skeleton
                Body body = Util.GetClosestBody(_bodies);
                //Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                // Process the tracked body
                if (body != null && body.IsTracked)
                {
                    // Get the upper skeleton
                    skeleton = Util.GetSkeletonUpperBody(sensor.Mapper, body);

                } // EndIf


                // Write the skeleton position in a file
                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".csv";
                Util.Skeleton2File(filename, skeleton);

                fileCount++;

            }// EndForEach

            Console.WriteLine("Saved {0} Body Data Files", fileCount - 1);
        } //End Method

        public static void SaveFaceData(this DepthSensorKinect2 sensor, string _filenamePattern)
        {
            List<FaceData> _listFaceData = sensor.Data.AllFaceData;

            int fileCount = 1;
            string filename;

            /// Traverse the skeleton list
            foreach (FaceData faceData in _listFaceData)
            {
                // Write face position in a unique file
                filename = _filenamePattern + MathFunc.ToFourDigits(fileCount) + ".csv";
                try
                {
                    using (StreamWriter sw = new StreamWriter(filename))
                    {
                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                                    "ColorSpace",
                                    faceData.boxColor.topX,
                                    faceData.boxColor.topY,
                                    faceData.boxColor.width,
                                    faceData.boxColor.height
                                    );

                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                                    "DepthSpace",
                                    faceData.boxDepth.topX,
                                    faceData.boxDepth.topY,
                                    faceData.boxDepth.width,
                                    faceData.boxDepth.height
                                    );

                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Cannot write {0}", filename);
                }

                fileCount++;

            }// EndForEach

            Console.WriteLine("Saved {0} Face Data Files", fileCount - 1 );
        } //End Method

        #endregion

    }
}

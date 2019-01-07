using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using KinectV2_Fingerspelling.Controllers;

namespace KinectV2_Fingerspelling.Extensions
{
    public static class Util
    {
        #region Save_Screenshot

        static public void SaveImageShotPNG(WriteableBitmap bitmap, string _path, string _source)
        {
            string filename = System.IO.Path.Combine(_path + "-" + _source + ".png");

            if (bitmap != null)
            {

                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Error saving screenshot: {0}", filename);
                }

                // Remove  the inserted frame
                encoder.Frames.Clear();
            }
        } // EndMethod

        
        static public void SaveImageShotJPEG(WriteableBitmap bitmap, string _path, string _source)
        {
            string filename = System.IO.Path.Combine(_path + "-" + _source + ".jpeg");

            if (bitmap != null)
            {

                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new JpegBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Error saving screenshot: {0}", filename);
                }
                // Remove  the inserted frame
                encoder.Frames.Clear();

            }
        } // EndMethod

        
        public static void SaveBodyShotJoints(IList<Body> _listBodies, CoordinateMapper mapper, string _path, string _source)
        {

            SkeletonOfBody skeleton = new SkeletonOfBody(Constants.SKEL_TOTAL_JOINTS);
            string filename = System.IO.Path.Combine(_path + "-" + _source + ".csv");

            Body body = Util.GetClosestBody(_listBodies);
                       
            
            // Process the tracked body
            if (body != null && body.IsTracked)
            {
                    // Get the complete skeleton
                    skeleton = Util.GetSkeletonBody(mapper, body);
            } // EndIf

            // Write the skeleton position in a file
            Util.Skeleton2File(filename, skeleton);

        } //EndMethod

        public static void SaveBodyShotUpperJoints(IList<Body> _listBodies, CoordinateMapper mapper, string _path, string _source)
        {

            SkeletonOfBody skeleton = new SkeletonOfBody(Constants.SKEL_UP_TOTAL_JOINTS);
            string filename = System.IO.Path.Combine(_path + "-" + _source + ".csv");

            Body body = Util.GetClosestBody(_listBodies);


            // Process the tracked body
            if (body != null && body.IsTracked)
            {
                // Get the complete skeleton
                skeleton = Util.GetSkeletonUpperBody (mapper, body);
            } // EndIf

            // Write the skeleton position in a file
            Util.Skeleton2File(filename, skeleton);

        } //EndMethod
               
        public static void SaveFaceShot(FaceData faceData, string _path, string _source)
        {
            string filename = System.IO.Path.Combine(_path + "-" + _source + ".csv");
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

        } //EndMethod

        #endregion


        #region ToWriteableBitmaps

        public static WriteableBitmap ToWriteableBitmap(ColorImageSize viewColorSize, byte[] buff32)
        {
            // Create a temporal bitmap to save
            WriteableBitmap picture = new WriteableBitmap(
                viewColorSize.Width,
                viewColorSize.Height,
                96.0, 96.0, PixelFormats.Bgr32, null);

            // Copy array data into the writeable bitmap
            picture.Lock();
            picture.WritePixels(new Int32Rect(0, 0, viewColorSize.Width, viewColorSize.Height),
                    buff32, viewColorSize.Width * 4, 0);
            picture.Unlock();

            return picture;
        }

        public static WriteableBitmap ToWriteableBitmap(ColorImageSize viewColorSize, ushort[] buff16)
        {
            // Create a temporal bitmap to save
            WriteableBitmap picture = new WriteableBitmap(
                viewColorSize.Width,
                viewColorSize.Height,
                96.0, 96.0, PixelFormats.Gray16, null);

            // Copy array data into the writeable bitmap
            picture.Lock();
            picture.WritePixels(new Int32Rect(0, 0, viewColorSize.Width, viewColorSize.Height),
                    buff16, viewColorSize.Width * 2, 0);
            picture.Unlock();

            return picture;
        }

        public static WriteableBitmap ToWriteableBitmap(DepthImageSize viewDepthSize, byte[] buff32)
        {
            // Create a temporal bitmap to save
            WriteableBitmap picture = new WriteableBitmap(
                viewDepthSize.Width,
                viewDepthSize.Height,
                96.0, 96.0, PixelFormats.Bgr32, null);

            // Copy array data into the writeable bitmap
            picture.Lock();
            picture.WritePixels(new Int32Rect(0, 0, viewDepthSize.Width, viewDepthSize.Height),
                    buff32, viewDepthSize.Width * 4, 0);
            picture.Unlock();

            return picture;
        }

        public static WriteableBitmap ToWriteableBitmap(DepthImageSize viewDepthSize, ushort[] buff16)
        {
            // Create a temporal bitmap to save
            WriteableBitmap picture = new WriteableBitmap(
                viewDepthSize.Width,
                viewDepthSize.Height,
                96.0, 96.0, PixelFormats.Gray16, null);

            // Copy array data into the writeable bitmap
            picture.Lock();
            picture.WritePixels(new Int32Rect(0, 0, viewDepthSize.Width, viewDepthSize.Height),
                    buff16, viewDepthSize.Width * 2, 0);
            picture.Unlock();

            return picture;
        }     

        public static byte[] MapDepthToColor(DepthSensorKinect2 sensor,
           byte[] colorData, ushort[] depthData, byte[] bodyIndexData)
        {
            ColorSpacePoint[] depthToColorSpacePoints = new ColorSpacePoint[sensor.DepthImageSize.Width * sensor.DepthImageSize.Height];
            byte[] buffData32 = new byte[sensor.DepthImageSize.Width * sensor.DepthImageSize.Height * 4];

            // Coordinate mapping
            sensor.Mapper.MapDepthFrameToColorSpace(depthData, depthToColorSpacePoints);

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

            return buffData32;
        }

        public static ushort[] MapColorToDepth(DepthSensorKinect2 sensor,
           byte[] colorData, ushort[] depthData, byte[] bodyIndexData)
        {
            DepthSpacePoint[] colorToDepthSpacePoints = new DepthSpacePoint[sensor.ColorImageSize.Width * sensor.ColorImageSize.Height];
            ushort[] buffData16 = new ushort[sensor.ColorImageSize.Width * sensor.ColorImageSize.Height];

            // Coordinate mapping
            sensor.Mapper.MapColorFrameToDepthSpace(depthData, colorToDepthSpacePoints);

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

            return buffData16;
        }

        #endregion


        #region PlotMarkups

        public static void WriteFaceOverFrame(DepthSensorKinect2 sensor, VisTypes visType, BoxFace boxFace, int sizeLine, ref byte[] buffColor32)
        {
            // Box = (topX, topY, width, height)
            // Box = new BoxFace(boxColor.Left, boxColor.Top, (boxColor.Right - boxColor.Left), (boxColor.Bottom - boxColor.Top));
            int _left = boxFace.topX;
            int _right = boxFace.width + _left;
            int _top = boxFace.topY;
            int _bottom = boxFace.height + _top;

            switch (visType)
            {
                case VisTypes.Color:

                    for (int ii = _left; ii <= _right; ii++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posX = ii + a;
                                int posY1 = _top + b;
                                int posY2 = _bottom + b;


                                if (
                                    (posX >= 0 && posX < sensor.ColorImageSize.Width) &&
                                    (posY1 >= 0 && posY1 < sensor.ColorImageSize.Height) &&
                                    (posY2 >= 0 && posY2 < sensor.ColorImageSize.Height)
                                    )
                                {
                                    // top line 
                                    int indexRGBA1 = (posY1 * sensor.ColorImageSize.Width + posX) * 4;
                                    buffColor32[indexRGBA1 + 0] = 0x0; // B
                                    buffColor32[indexRGBA1 + 1] = 0x0; // G 
                                    buffColor32[indexRGBA1 + 2] = 0xff; // R 
                                    // bottom line 
                                    int indexRGBA2 = (posY2 * sensor.ColorImageSize.Width + posX) * 4;
                                    buffColor32[indexRGBA2 + 0] = 0x0; // B
                                    buffColor32[indexRGBA2 + 1] = 0x0; // G 
                                    buffColor32[indexRGBA2 + 2] = 0xff; // R 
                                }
                            }
                    } //EndFor

                    for (int jj = _top; jj <= _bottom; jj++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posY = jj + b;
                                int posX1 = _left + a;
                                int posX2 = _right + a;

                                if (
                                    (posX1 >= 0 && posX1 < sensor.ColorImageSize.Width) &&
                                    (posY >= 0 && posY < sensor.ColorImageSize.Height) &&
                                    (posX2 >= 0 && posX2 < sensor.ColorImageSize.Width)
                                    )
                                {
                                    // left line        
                                    int indexRGBA1 = (posY * sensor.ColorImageSize.Width + posX1) * 4;
                                    buffColor32[indexRGBA1 + 0] = 0x0; //B
                                    buffColor32[indexRGBA1 + 1] = 0x0;// G 
                                    buffColor32[indexRGBA1 + 2] = 0xff; // R 
                                    // right line   
                                    int indexRGBA2 = (posY * sensor.ColorImageSize.Width + posX2) * 4;
                                    buffColor32[indexRGBA2 + 0] = 0x0; //B
                                    buffColor32[indexRGBA2 + 1] = 0x00;// G 
                                    buffColor32[indexRGBA2 + 2] = 0xff; // R 
                                }
                            }
                    } //EndFor

                    break;

                case VisTypes.Depth:
                case VisTypes.Infrared:
                case VisTypes.BodyIndex:

                    for (int ii = _left; ii <= _right; ii++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posX = ii + a;
                                int posY1 = _top + b;
                                int posY2 = _bottom + b;

                                if (
                                    (posX >= 0 && posX < sensor.DepthImageSize.Width) &&
                                    (posY1 >= 0 && posY1 < sensor.DepthImageSize.Height) &&
                                    (posY2 >= 0 && posY2 < sensor.DepthImageSize.Height)
                                    )
                                {
                                    // top line        
                                    int indexRGBA1 = (posY1 * sensor.DepthImageSize.Width + posX) * 4;
                                    buffColor32[indexRGBA1 + 0] = 0x0; // B
                                    buffColor32[indexRGBA1 + 1] = 0x0; // G 
                                    buffColor32[indexRGBA1 + 2] = 0xff; // R 
                                    // bottom line 
                                    int indexRGBA2 = (posY2 * sensor.DepthImageSize.Width + posX) * 4;
                                    buffColor32[indexRGBA2 + 0] = 0x0; // B
                                    buffColor32[indexRGBA2 + 1] = 0x0; // G 
                                    buffColor32[indexRGBA2 + 2] = 0xff; // R 

                                }
                            }
                    } //EndFor

                    for (int jj = _top; jj <= _bottom; jj++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posY = jj + b;
                                int posX1 = _left + a;
                                int posX2 = _right + a;


                                if (
                                    (posX1 >= 0 && posX1 < sensor.DepthImageSize.Width) &&
                                    (posY >= 0 && posY < sensor.DepthImageSize.Height) &&
                                    (posX2 >= 0 && posX2 < sensor.DepthImageSize.Width)
                                    )
                                {
                                    // left line               
                                    int indexRGBA1 = (posY * sensor.DepthImageSize.Width + posX1) * 4;
                                    buffColor32[indexRGBA1 + 0] = 0x0; //B
                                    buffColor32[indexRGBA1 + 1] = 0x0;// G 
                                    buffColor32[indexRGBA1 + 2] = 0xff; // R 
                                    // right line    
                                    int indexRGBA2 = (posY * sensor.DepthImageSize.Width + posX2) * 4;
                                    buffColor32[indexRGBA2 + 0] = 0x0; //B
                                    buffColor32[indexRGBA2 + 1] = 0x00;// G 
                                    buffColor32[indexRGBA2 + 2] = 0xff; // R 
                                }
                            }
                    } //EndFor

                    break;

                case VisTypes.None:
                    break;

                default:
                    break;
            }

        }

        public static void WriteFaceOverFrame(DepthSensorKinect2 sensor, VisTypes visType, BoxFace boxFace, int sizeLine, ref ushort[] buffDepth16)
        {
            // Box = (topX, topY, width, height)
            // Box = new BoxFace(boxColor.Left, boxColor.Top, (boxColor.Right - boxColor.Left), (boxColor.Bottom - boxColor.Top));
            int _left = boxFace.topX;
            int _right = boxFace.width + _left;
            int _top = boxFace.topY;
            int _bottom = boxFace.height + _top;

            switch (visType)
            {
                case VisTypes.Color:

                    for (int ii = _left; ii <= _right; ii++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posX = ii + a;
                                int posY1 = _top + b;
                                int posY2 = _bottom + b;


                                if (
                                    (posX >= 0 && posX < sensor.ColorImageSize.Width) &&
                                    (posY1 >= 0 && posY1 < sensor.ColorImageSize.Height) &&
                                    (posY2 >= 0 && posY2 < sensor.ColorImageSize.Height)
                                    )
                                {
                                    // top line 
                                    int indexDepth1 = posY1 * sensor.ColorImageSize.Width + posX;
                                    buffDepth16[indexDepth1] = 0xffff;

                                    // bottom line 
                                    int indexDepth2 = posY2 * sensor.ColorImageSize.Width + posX;
                                    buffDepth16[indexDepth2] = 0xffff;

                                }
                            }
                    } //EndFor

                    for (int jj = _top; jj <= _bottom; jj++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posY = jj + b;
                                int posX1 = _left + a;
                                int posX2 = _right + a;

                                if (
                                    (posX1 >= 0 && posX1 < sensor.ColorImageSize.Width) &&
                                    (posY >= 0 && posY < sensor.ColorImageSize.Height) &&
                                    (posX2 >= 0 && posX2 < sensor.ColorImageSize.Width)
                                    )
                                {
                                    // left line        
                                    int indexDepth1 = posY * sensor.ColorImageSize.Width + posX1;
                                    buffDepth16[indexDepth1] = 0xffff;
                                    // right line   
                                    int indexDepth2 = posY * sensor.ColorImageSize.Width + posX2;
                                    buffDepth16[indexDepth2] = 0xffff;
                                }
                            }
                    } //EndFor

                    break;

                case VisTypes.Depth:
                case VisTypes.Infrared:
                case VisTypes.BodyIndex:

                    for (int ii = _left; ii <= _right; ii++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posX = ii + a;
                                int posY1 = _top + b;
                                int posY2 = _bottom + b;

                                if (
                                    (posX >= 0 && posX < sensor.DepthImageSize.Width) &&
                                    (posY1 >= 0 && posY1 < sensor.DepthImageSize.Height) &&
                                    (posY2 >= 0 && posY2 < sensor.DepthImageSize.Height)
                                    )
                                {
                                    // top line        
                                    int indexDepth1 = posY1 * sensor.DepthImageSize.Width + posX;
                                    buffDepth16[indexDepth1] = 0xffff;
                                    // bottom line 
                                    int indexDepth2 = posY2 * sensor.DepthImageSize.Width + posX;
                                    buffDepth16[indexDepth2] = 0xffff;

                                }
                            }
                    } //EndFor

                    for (int jj = _top; jj <= _bottom; jj++)
                    {
                        for (int a = -sizeLine; a <= sizeLine; a++)
                            for (int b = -sizeLine; b <= sizeLine; b++)
                            {
                                int posY = jj + b;
                                int posX1 = _left + a;
                                int posX2 = _right + a;


                                if (
                                    (posX1 >= 0 && posX1 < sensor.DepthImageSize.Width) &&
                                    (posY >= 0 && posY < sensor.DepthImageSize.Height) &&
                                    (posX2 >= 0 && posX2 < sensor.DepthImageSize.Width)
                                    )
                                {
                                    // left line               
                                    int indexDepth1 = posY * sensor.DepthImageSize.Width + posX1;
                                    buffDepth16[indexDepth1] = 0xffff;
                                    // right line    
                                    int indexDepth2 = posY * sensor.DepthImageSize.Width + posX2;
                                    buffDepth16[indexDepth2] = 0xffff;
                                }
                            }
                    } //EndFor

                    break;

                case VisTypes.None:
                    break;

                default:
                    break;
            }

        }

        public static void WriteSkeletonOverFrame(DepthSensorKinect2 sensor, VisTypes visType, SkeletonOfBody skeleton, int sizePoint, ref byte[] buffColor32)
        {
            // Draw joints pixels over the color image

            switch (visType)
            {
                case VisTypes.Color:

                    // Overwrite the points over the color image  in the color space
                    foreach (var pos in skeleton.jointColorSpace)
                    {
                        for (int m = -sizePoint; m <= sizePoint; m++)
                            for (int n = -sizePoint; n <= sizePoint; n++)
                            {

                                int posX = (int)pos.X + m;
                                int posY = (int)pos.Y + n;

                                if ((posX >= 0 && posX < sensor.ColorImageSize.Width) && (posY >= 0 && posY < sensor.ColorImageSize.Height))
                                {
                                    int indexRGBA = (posY * sensor.ColorImageSize.Width + posX) * 4;
                                    // Highlight the pixel
                                    buffColor32[indexRGBA + 0] = 0; //B
                                    buffColor32[indexRGBA + 1] = 0xff;// G = 255
                                    buffColor32[indexRGBA + 2] = 0xff; // R = 255
                                }
                            }
                    }


                    break;

                case VisTypes.Depth:
                case VisTypes.Infrared:
                case VisTypes.BodyIndex:

                    // Overwrite the points over the color image  in the depth space
                    foreach (var pos in skeleton.jointDepthSpace)
                    {
                        for (int m = -sizePoint; m <= sizePoint; m++)
                            for (int n = -sizePoint; n <= sizePoint; n++)
                            {

                                int posX = (int)pos.X + m;
                                int posY = (int)pos.Y + n;

                                if ((posX >= 0 && posX < sensor.DepthImageSize.Width) && (posY >= 0 && posY < sensor.DepthImageSize.Height))
                                {
                                    int indexRGBA = (posY * sensor.DepthImageSize.Width + posX) * 4;
                                    // Highlight the pixel
                                    buffColor32[indexRGBA + 0] = 0; //B
                                    buffColor32[indexRGBA + 1] = 0xff;// G = 255
                                    buffColor32[indexRGBA + 2] = 0xff; // R = 255
                                }
                            }
                    }
                    break;

                case VisTypes.None:
                    break;

                default:
                    break;
            }
        }

        public static void WriteSkeletonOverFrame(DepthSensorKinect2 sensor, VisTypes visType, SkeletonOfBody skeleton, int sizePoint, ref ushort[] buffDepth16)
        {
            // Draw joints pixels over the depth image            
            switch (visType)
            {
                case VisTypes.Color:

                    // Overwrite the points over the depth image in color space
                    foreach (var pos in skeleton.jointColorSpace)
                    {
                        for (int m = -sizePoint; m <= sizePoint; m++)
                            for (int n = -sizePoint; n <= sizePoint; n++)
                            {

                                int posX = (int)pos.X + m;
                                int posY = (int)pos.Y + n;

                                if ((posX >= 0 && posX < sensor.ColorImageSize.Width) && (posY >= 0 && posY < sensor.ColorImageSize.Height))
                                {
                                    int indexDepth = posY * sensor.ColorImageSize.Width + posX;
                                    // Highlight the pixel
                                    buffDepth16[indexDepth] = 0xffff;
                                }
                            }
                    }
                    break;

                case VisTypes.Depth:
                case VisTypes.Infrared:
                case VisTypes.BodyIndex:

                    // Overwrite the points over the depth image in depth space             
                    foreach (var pos in skeleton.jointDepthSpace)
                    {
                        for (int m = -sizePoint; m <= sizePoint; m++)
                            for (int n = -sizePoint; n <= sizePoint; n++)
                            {

                                int posX = (int)pos.X + m;
                                int posY = (int)pos.Y + n;

                                if ((posX >= 0 && posX < sensor.DepthImageSize.Width) && (posY >= 0 && posY < sensor.DepthImageSize.Height))
                                {
                                    int indexDepth = posY * sensor.DepthImageSize.Width + posX;
                                    // Highlight the pixel
                                    buffDepth16[indexDepth] = 0xffff;

                                }
                            }
                    }
                    break;

                case VisTypes.None:
                    break;

                default:
                    break;
            }
        }

        #endregion


        #region Skeleton_Utils

        public static SkeletonOfBody GetSkeletonBody(CoordinateMapper mapper, Body body)
        {
            // Skeleton structure
            SkeletonOfBody skel = new SkeletonOfBody(Constants.SKEL_TOTAL_JOINTS);
            
            // Process the tracked body
            if (body != null && body.IsTracked)
            {
                //Extract body information
                IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                IReadOnlyDictionary<JointType, JointOrientation> orientations = body.JointOrientations;
                
                foreach (JointType jointType in joints.Keys)
                {
                    Point pointInColor = MathFunc.ToPoint(mapper, VisTypes.Color, joints[jointType].Position);
                    Point pointInDepth = MathFunc.ToPoint(mapper, VisTypes.Depth, joints[jointType].Position);

                    // Discard joint that are not being tracked
                    if (joints[jointType].TrackingState == TrackingState.Tracked || joints[jointType].TrackingState == TrackingState.Inferred)
                    {
                        skel.jointName[(int)jointType] = jointType.ToString().Trim(); // Name
                        skel.jointColorSpace[(int)jointType] = pointInColor; // 2D Point
                        skel.jointDepthSpace[(int)jointType] = pointInDepth; // 2D Point
                        skel.jointCameraSpace[(int)jointType] = joints[jointType].Position; // 3D Point
                        skel.jointQuaternion[(int)jointType] = orientations[jointType].Orientation; // Vector 4D
                    }
                }// EndForEach

            } // EndIf

            return skel;

        } // EndMethod

        public static SkeletonOfBody GetSkeletonUpperBody(CoordinateMapper mapper, Body body)
        {
            // Skeleton structure
            SkeletonOfBody skel = new SkeletonOfBody(Constants.SKEL_UP_TOTAL_JOINTS);
            
            // Process the tracked body
            if (body != null && body.IsTracked)
            {
                //Extract body information
                IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                IReadOnlyDictionary<JointType, JointOrientation> orientations = body.JointOrientations;

                int cont = 0;
                foreach (var pos in Constants.SKEL_UP_INDEX)
                {
                    Point pointInColor = MathFunc.ToPoint(mapper, VisTypes.Color, joints[(JointType)pos].Position);
                    Point pointInDepth = MathFunc.ToPoint(mapper, VisTypes.Depth, joints[(JointType)pos].Position);

                    // Discard joint that are not being tracked
                    if (joints[(JointType)pos].TrackingState == TrackingState.Tracked || joints[(JointType)pos].TrackingState == TrackingState.Inferred)
                    {
                        skel.jointName[cont] = joints[(JointType)pos].JointType.ToString().Trim(); // Name
                        skel.jointColorSpace[cont] = pointInColor; // 2D Point
                        skel.jointDepthSpace[cont] = pointInDepth; // 2D Point
                        skel.jointCameraSpace[cont] = joints[(JointType)pos].Position; // 3D Point
                        skel.jointQuaternion[cont] = orientations[(JointType)pos].Orientation; // Vector 4D
                    }
                    cont++;
                }// EndForEach
            } // EndIf

            return skel;

        } // EndMethod

        public static Body GetClosestBody(IList<Body> bodies)
        {
            Body body = null;
            int activeBodyIndex = -1; // Default to impossible value

            // Iterate through all bodies, 
            // no point persisting activeBodyIndex because must 
            // compare with depth of all bodies so no gain in efficiency.

            float minZPoint = float.MaxValue; // Default to impossible value
            for (int i = 0; i < Constants.NUM_BODIES; i++)
            {
                body = bodies[i];
                if (body.IsTracked)
                {
                    float zMeters =
                       body.Joints[JointType.Head].Position.Z; //Based on the head
                    if (zMeters < minZPoint)
                    {
                        minZPoint = zMeters;
                        activeBodyIndex = i;
                    }
                }
            }

            // Return the selected body
            if (activeBodyIndex != -1)
            {
                body = bodies[activeBodyIndex];
            }
            
            return body;

        }// EndMethod

        public static void Skeleton2File(string _filename, SkeletonOfBody skeleton)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(_filename))
                {
                    for (int i = 0; i < skeleton.jointName.Count(); i++)
                    {
                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                            skeleton.jointName[i],
                            skeleton.jointColorSpace[i].X,
                            skeleton.jointColorSpace[i].Y,
                            0,
                            0
                            );

                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                            skeleton.jointName[i],
                            skeleton.jointDepthSpace[i].X,
                            skeleton.jointDepthSpace[i].Y,
                            0,
                            0
                            );

                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                              skeleton.jointName[i],
                            skeleton.jointCameraSpace[i].X,
                            skeleton.jointCameraSpace[i].Y,
                            skeleton.jointCameraSpace[i].Z,
                            0
                            );

                        sw.WriteLine("{0}; {1}; {2}; {3}; {4}",
                              skeleton.jointName[i],
                            skeleton.jointQuaternion[i].X,
                            skeleton.jointQuaternion[i].Y,
                            skeleton.jointQuaternion[i].Z,
                            skeleton.jointQuaternion[i].W
                            );
                    }

                }
            }
            catch (IOException)
            {
                Console.WriteLine("Cannot write {0}", _filename);
            }

        }

        #endregion


        #region Update_Bitmaps

        public static void UpdateOutBitmap(WriteableBitmap bitmap, ushort[] buff)
        {
            bitmap.Lock();
            bitmap.WritePixels(
                new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                    buff, bitmap.PixelWidth * 2, 0);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();
        }

        public static void UpdateOutBitmap(WriteableBitmap bitmap, int stride, byte[] buff)
        {
            bitmap.Lock();
            bitmap.WritePixels(
                new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
                    buff, bitmap.PixelWidth * stride, 0);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();
        }

        #endregion

        
    }
}
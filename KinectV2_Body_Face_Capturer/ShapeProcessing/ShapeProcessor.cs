using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectV2_Fingerspelling.Controllers;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

//using MathNet.Numerics.LinearAlgebra;

namespace KinectV2_Fingerspelling.ShapeProcessing
{
    public static class ShapeProcessor
    {
        #region processor_methods

        //public ShapeProcessor()
        //{
        //}

        //~ShapeProcessor()
        //{
        //}
                

        // Threshold the depth data returning an array of markups
        public static byte[] HandThresholding(DepthImageSize _imageSize, int _col, int _row, int _th, ushort[] _frameDepth16)
        {

            byte[] mask8 = new byte[_frameDepth16.Length];

            Array.Clear(mask8, 0, mask8.Length);
            int handDepth = _frameDepth16[_row * _imageSize.Width + _col];

            int thUp = handDepth + _th;
            int thLow = handDepth - _th;

            for (int i = 0; i < _frameDepth16.Length; i++)
            {
                int valDepth = _frameDepth16[i];
                if (valDepth > thLow && valDepth < thUp)
                {
                    mask8[i] = 1; //255
                }
            }

            return mask8;
        }


        public static System.Drawing.Point PixelLocation(int _row, int _col)
        {
            return new System.Drawing.Point(_row, _col);
        }

        public static void RecursiveGrowing(System.Drawing.Point seed, Image<Gray, Byte> _img, Image<Gray, Byte> _mask)
        {
            Emgu.CV.Matrix<byte> mat1 = new Emgu.CV.Matrix<Byte>(_img.Height, _img.Width);
            Emgu.CV.Matrix<Byte> mat2 = new Emgu.CV.Matrix<Byte>(_img.Height, _img.Width);
            mat1.SetZero();
            mat2.SetZero();
                                                
            //Emgu.CV.Structure.MCvConnectedComp conn = new Emgu.CV.Structure.MCvConnectedComp();


            //CvInvoke.FloodFill(_img, mask, seed, new MCvScalar(128), out rect, new MCvScalar(1), new MCvScalar(1), Emgu.CV.CvEnum.Connectivity.EightConnected);

            //Console.WriteLine("Rectangle {0},{1}", rect.Width, rect.Height);

            //Emgu.CV.Structure.MCvSeq components = new Emgu.CV.Structure.MCvConnectedComp <MCvConnectedComp>(new MemStorage());

            //  Emgu.CV.CvInvoke.cvPyrSegmentation(image, result, storage, out components._ptr, 4, 255, 30);
            //CvInvoke.sh

            

            // Set the seed
            _mask.Data[seed.X, seed.Y, 0] = 1; // Set to 1

            mat1[seed.X, seed.Y] = 1;
            mat2[seed.X, seed.Y] = 1;            
            
            
            int iter = 0;
            while (iter < 1)
            {

                // Dilation
                _mask.Dilate(1);
                                                
                //System.Drawing.Point[] points = GetNeighbors(System.Drawing.Point loc, System.Drawing.Size size)
            
                iter++;

            }



            // recurse left            
            //_row = _row - 1;
            //if ((_imData[_row, _col].Intensity != 0) && (_imMask[_row, _col].Intensity == 0))
            //{
            //    RecursiveGrowing(_row, _col, _imData, _imMask);
            //}

            // recurse right 
            //_row = _row - 1;
            //if ((_img[_row, _col].Intensity != 0) && (_imMask[_row, _col].Intensity == 0))
            //{
            //    RecursiveGrowing(_row, _col, _img, _imMask);
            //}

            //// recurse up 
            //_col = _col - 1;
            //_row = _row - 1;
            //if ((_imData[_row, _col].Intensity != 0) && (_imMask[_row, _col].Intensity == 0))
            //{
            //    RecursiveGrowing(_row, _col, _imData, _imMask);
            //}

            // recurse down
            //_col = _col - 1;
            //if ((_img[_row, _col].Intensity != 0) && (_imMask[_row, _col].Intensity == 0))
            //{
            //    RecursiveGrowing(_row, _col, _img, _imMask);
            //}

        }

       
        public static System.Drawing.Point[] GetNeighbors(System.Drawing.Point loc, System.Drawing.Size size)
        {
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            // top
            if (loc.Y > 0)
                points.Add(new System.Drawing.Point(loc.X, loc.Y - 1));
            // bottom
            if (loc.Y < size.Height - 1)
                points.Add(new System.Drawing.Point(loc.X, loc.Y + 1));
            // left
            if (loc.X > 0)
                points.Add(new System.Drawing.Point(loc.X - 1, loc.Y));
            // right
            if (loc.X < size.Width - 1)
                points.Add(new System.Drawing.Point(loc.X + 1, loc.Y));

            return points.ToArray();
        }

        #endregion


    }
}

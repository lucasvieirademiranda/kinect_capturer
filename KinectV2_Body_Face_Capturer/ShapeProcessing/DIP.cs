using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;

namespace KinectV2_Fingerspelling.ShapeProcessing
{
    /// <summary>
    /// Image Processing tools
    /// </summary>
    public static class DIP
    {
        // Copy an array buffer to another
        public static T[] BufferClone<T>(T[] buffSrc)
        {
            T[] buffDst = new T[buffSrc.Length];
            //Buffer.BlockCopy(buffSrc, 0, buffDst, 0, buffSrc.Length); // how many of bytes to copy ?                        
            Array.Copy(buffSrc, buffDst, buffSrc.Length); // how many elements?
            return buffDst;
        }

        // Copy an array buffer to another
        public static void BufferClone<T>(T[] buffSrc, T[] buffDst)
        {            
            Array.Clear(buffDst, 0, buffDst.Length);
            //Buffer.BlockCopy(buffSrc, 0, buffDst, 0, buffSrc.Length); // how many of bytes to copy ?            
            Array.Copy(buffSrc, buffDst, buffSrc.Length); // how many elements?
        }

        // Multiply all buffer elements by a scalar value
        public static void BufferMultiplyRange(byte[] buffArr, float scalar)
        {
            int i = 0;
            Array.ForEach(buffArr, (x) => { buffArr[i++] = (byte)(x * scalar); });
        }

        // Multiply all buffer elements by a scalar value
        public static void BufferMultiplyRange(ushort[] buffArr, float scalar)
        {
            int i = 0;
            Array.ForEach(buffArr, (x) => { buffArr[i++] = (ushort)(x * scalar); });            
        }
               
        // Convert the Array values into logical values using a threshold
        public static void Buffer2Logical(byte[] buffArr, int th)
        {
            int i = 0;
            Array.ForEach(buffArr, (x) => { buffArr[i++] = (byte)(x > th ? 0 : 1); });            
        }
                        
        // Convert Array buffer to Image<>
        public static Image<Gray, byte> Array2Image(int _width, int _height, byte[] _gray8)
        {
            Image<Gray, byte> imGray8 = new Image<Gray, byte>(_width, _height);
            //imGray8.SetZero();

            imGray8.Bytes = _gray8;  // Set an array of bytes
            return imGray8;
        }

        // Convert Image<> to Array buffer
        public static byte[] Image2Array(Image<Gray, byte> _imGray8)
        {
            byte[] gray8 = new byte[_imGray8.Width * _imGray8.Height];

            gray8 = _imGray8.Bytes;  // Get an array of bytes            
            return gray8;
        }

        // Convert Array buffer to Matrix
        public static Matrix<byte> Array2Matrix(int _rows, int _cols, byte[] _gray8)
        {
            Matrix<byte> matrix8 = new Matrix<byte>(_rows, _cols);

            //matrix.SetZero();
            //Console.WriteLine("Size {0}", matrix8.Size);
            matrix8.Bytes = _gray8;  // Set an array of bytes
            return matrix8;
        }
               

         //for (int j = (handX - 10); j < (handX + 10); j++)
         //           {
         //               for (int i = (handX - 10); i < (handX + 10); i++)
         //               {
         //                   Console.Write(mat1[j, i] + " ");
         //               }
         //               Console.WriteLine();
         //           }

        
    }
}

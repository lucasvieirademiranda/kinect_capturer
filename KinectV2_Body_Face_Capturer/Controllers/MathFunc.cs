
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows;

namespace KinectV2_Fingerspelling.Controllers
{
    public static class MathFunc
    {
        /// <summary>
        /// Converts the specified 3D CameraSpacePoint into a 2D ImageSpacePoint.        
        /// </summary>
        /// <param name="visType">The type of the conversion (color, depth, infrared, or bodyindex).</param>
        /// <param name="position3D">The CameraSpacePoint to convert.</param>
        /// <param name="coordinateMapper">The CoordinateMapper to make the conversion.</param>
        /// <returns>The corresponding 2D integer point.</returns>
        public static Point ToPoint(CoordinateMapper coordinateMapper, VisTypes visType, CameraSpacePoint position3D)
        {
            //System.Drawing.
            Point point = new Point(0, 0);
            switch (visType)
            {
                case VisTypes.Color:
                    {
                        // SDK Coordinate mapping
                        ColorSpacePoint colorPoint = coordinateMapper.MapCameraPointToColorSpace(position3D);
                        // The sentinel value is (-Inf, -Inf), meaning that no depth pixel corresponds to this color pixel.                               
                        point.X = float.IsNegativeInfinity(colorPoint.X) ? 0 : (int)(colorPoint.X + 0.5f);
                        point.Y = float.IsNegativeInfinity(colorPoint.Y) ? 0 : (int)(colorPoint.Y + 0.5f);

                    }
                    break;

                case VisTypes.Depth:
                case VisTypes.Infrared:
                case VisTypes.BodyIndex:
                    {
                        DepthSpacePoint depthPoint = coordinateMapper.MapCameraPointToDepthSpace(position3D);
                        point.X = float.IsNegativeInfinity(depthPoint.X) ? 0 : (int)(depthPoint.X + 0.5f);
                        point.Y = float.IsNegativeInfinity(depthPoint.Y) ? 0 : (int)(depthPoint.Y + 0.5f);
                    }
                    break;

                case VisTypes.None:
                    break;

                default:
                    break;
            }

            return point;
        }


        /// <summary>
        /// Set a number in a four digit format
        /// </summary>
        /// <param name="number"> number </param>
        /// <returns></returns>
        public static string ToFourDigits(int number)
        {
            return number.ToString("".PadLeft(4, '0'));

        }
    }
}

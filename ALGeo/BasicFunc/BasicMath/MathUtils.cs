using Plankton;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ALDGP
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Triplet
    {
        public int i { get; set; }
        public int j { get; set; }
        public double v { get; set; }
        public Triplet(int i, int j, double v)
        {
            this.i = i;
            this.j = j;
            this.v = v;
        }
        public void SetValue(double v)
        {
            this.v = v;
        }
    }
    public class MathUtils
    {
        /// <summary>
        /// Clamp cosine values as if angles are in [1, 179].
        /// </summary>
        /// <param name="v">Input a cosine value.</param>
        /// <returns>Return a clamped cosine value.</returns>
        public static double Clamp_Cos(double v)
        {
            double bound = 0.9986f; // 3 degrees
            return (v < -bound ? -bound : (v > bound ? bound : v));
        }

        /// <summary>
        /// Clamp cotangent values as if angles are in [1, 89].
        /// </summary>
        /// <param name="v">Input a cotangent value.</param>
        /// <returns>Return a clamped cotangent value.</returns>
        public static double Clamp_Cot(double v)
        {
            double bound = 19.1f; // 3 degrees
            return (v< -bound? -bound : (v > bound? bound : v));
        }

        /// <summary>
        /// Compute the cotangent value of given two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>Return a cotangent value.</returns>
        public static double Cotan(Vector3D a, Vector3D b)
        {
            return a*b / a.CrossProduct(b).Length;
        }
    }
}

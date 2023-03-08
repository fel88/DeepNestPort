using System.Runtime.InteropServices;

namespace Minkowski
{
    public class MinkowskiWrapper
    {        
        [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setData(int cntA, double[] pntsA, int holesCnt, int[] holesSizes, double[] holesPoints, int cntB, double[] pntsB);

        [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void calculateNFP();
        
        [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getSizes1(int[] sizes);
        
        [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getSizes2(int[] sizes1, int[] sizes2);
        
        [DllImport("minkowski.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getResults(double[] data, double[] holesData);
    }
}

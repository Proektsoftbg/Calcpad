using System;

namespace Calcpad.Core
{
    internal static class HpFFT
    {   
        /*Replaces re[n], im[n] by its discrete Fourier transform, if isign is input as 1; 
        or replaces re[n], im[n] by n times its inverse discrete Fourier transform, if isign is input as −1.
        n MUST be an integer power of 2 (this is not checked for!).*/
        internal static void Transform(double[] re, double[] im, int isign)
        {
            int mmax, m, istep;
            double wtemp, wr, wpr, wpi, wi, theta, tempr, tempi;
            int n = Math.Min(re.Length, im.Length);
            int j = 0;
            for (int i = 0; i < n; ++i)
            {
                if (j > i)
                {
                    Swap(ref re[i], ref re[j]);
                    Swap(ref im[i], ref im[j]);
                }
                m = n >> 1;
                while (m >= 1 && j >= m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }
            //Here begins the Danielson-Lanczos section of the routine.
            mmax = 1;
            while (n > mmax)  //Outer loop executed log2 n times.
            {
                istep = mmax << 1;
                theta = isign * (Math.PI / mmax); //Initialize the trigonometric recurrence.
                wtemp = Math.Sin(0.5 * theta);
                wpr = -2.0 * wtemp * wtemp;
                wpi = Math.Sin(theta);
                wr = 1.0;
                wi = 0.0;
                for (m = 0; m < mmax; ++m)
                {
                    // Here are the two nested inner loops.
                    for (int i = m; i < n; i += istep)
                    {
                        j = i + mmax; //This is the Danielson - Lanczos formula
                        tempr = wr * re[j] - wi * im[j];
                        tempi = wr * im[j] + wi * re[j];
                        re[j] = re[i] - tempr;
                        im[j] = im[i] - tempi;
                        re[i] += tempr;
                        im[i] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr; //Trigonometric recurrence.
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
            static void Swap(ref double a, ref double b) => (b, a) = (a, b);
        }
    }
}

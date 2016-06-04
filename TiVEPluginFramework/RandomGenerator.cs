using System;
using JetBrains.Annotations;

namespace ProdigalSoftware.TiVEPluginFramework
{
    /// <summary>
    /// Generates random numbers from a uniform distribution using the Mersenne Twister algorithm (version MT19937). In addition to producing better random numbers, 
    /// this version is significantly faster than .Net's Random class.
    /// <para>MersenneTwister algorithm code taken from: https://en.wikipedia.org/wiki/Mersenne_Twister (Python source)</para>
    /// <para>Additional code taken from: http://www.codeproject.com/Articles/164087/Random-Number-Generation (and very cleaned up)</para>
    /// </summary>
    [PublicAPI]
    public sealed class RandomGenerator
    {
        #region Constants/Member variables
        private const int N = 624;
        private const int M = 397;
        private const uint MatrixA = 0x9908B0DFU;
        private const uint UpperMask = 0x80000000U;
        private const uint LowerMask = 0x7FFFFFFFU;
        private const double GenDoubleEpsilon = 1.0 / 4294967295.0;
        private const double GenDoubleEpsilonToOne = 1.0 / 4294967296.0;
        private const double GenDoubleEpsilon53 = 1.0 / 9007199254740992.0;

        private readonly uint[] mag01 = { 0x0U, MatrixA };
        private readonly uint[] mt = new uint[N];
        private readonly object syncObj = new object();

        private int index = N;
        #endregion

        #region Constructors
        public RandomGenerator() : this(Environment.TickCount)
        {
        }

        public RandomGenerator(int seed)
        {
            mt[0] = (uint)seed;
            for (int i = 1; i < N; i++)
                mt[i] = (uint)(1812433253 * (mt[i - 1] ^ mt[i - 1] >> 30) + i);
        }
        #endregion

        #region Public methods
        public int Next()
        { 
            return genrand_int31(); 
        }

        public int Next(int maxValue)
        { 
            return Next(0, maxValue); 
        }

        public int Next(int minValue, int maxValue)
        {
            return (int)Math.Floor((maxValue - minValue) * genrand_real1() + minValue);
        }

        public float NextFloat()
        { 
            return (float)genrand_real2();
        }

        public float NextFloat(bool includeOne)
        {
            return includeOne ? (float)genrand_real1() : (float)genrand_real2();
        }

        public float NextFloatPositive()
        { 
            return (float)genrand_real3();
        }

        public double NextDouble()
        { 
            return genrand_real2();
        }

        public double NextDouble(bool includeOne)
        {
            return includeOne ? genrand_real1() : genrand_real2();
        }

        public double NextDoublePositive()
        { 
            return genrand_real3();
        }
        
        public double Next53BitRes()
        { 
            return genrand_res53(); 
        }
        #endregion

        #region Private helper methods
        private int genrand_int31()
        {
            return (int)(GenerateRandomUInt() >> 1); 
        }

        private double genrand_real1()
        {
            return GenerateRandomUInt() * GenDoubleEpsilon;
        }
        
        private double genrand_real2()
        {
            return GenerateRandomUInt() * GenDoubleEpsilonToOne;
        }

        private double genrand_real3()
        {
            return (GenerateRandomUInt() + 0.5) * GenDoubleEpsilonToOne;
        }

        private double genrand_res53()
        {
            uint a = GenerateRandomUInt() >> 5;
            uint b = GenerateRandomUInt() >> 6;
            return (a * 67108864.0 + b) * GenDoubleEpsilon53;
        }

        private uint GenerateRandomUInt()
        {
            if (index >= N)
                TwistFast();

            uint y = mt[index++];

            // Right shift by 11 bits
            y = y ^ y >> 11;
            // Shift y left by 7 and take the bitwise and of 2636928640
            y = y ^ y << 7 & 2636928640;
            // Shift y left by 15 and take the bitwise and of y and 4022730752
            y = y ^ y << 15 & 4022730752;
            // Right shift by 18 bits
            return y ^ y >> 18;
        }

        // Original Twist implementation from the Python implementation
        //private void Twist()
        //{
        //    for (int i = 0; i < N; i++)
        //    {
        //        // Get the most significant bit and add it to the less significant bits of the next number
        //        uint y = (mt[i] & UpperMask) + (mt[(i + 1) % N] & LowerMask);
        //        mt[i] = mt[(i + M) % N] ^ y >> 1;

        //        if (y % 2 != 0)
        //            mt[i] = mt[i] ^ MatrixA;
        //    }
        //    index = 0;
        //}

        /// <summary>
        /// Much faster (almost 3x) implementation from http://www.codeproject.com/Articles/164087/Random-Number-Generation which avoids the modulous operator
        /// </summary>
        private void TwistFast()
        {
            int kk;
            uint y;
            for (kk = 0; kk < N - M; kk++)
            {
                y = (mt[kk] & UpperMask) | (mt[kk + 1] & LowerMask);
                mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
            }
            for (; kk < N - 1; kk++)
            {
                y = (mt[kk] & UpperMask) | (mt[kk + 1] & LowerMask);
                mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
            }
            y = (mt[N - 1] & UpperMask) | (mt[0] & LowerMask);
            mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
            index = 0;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace raytracinginoneweekend
{
    public class CryptoRandom
    {
        private RNGCryptoServiceProvider _provider;
        public CryptoRandom()
        {
            _provider = new RNGCryptoServiceProvider();
        }

        public float NextFloat()
        {
            var data = new byte[sizeof(uint)];
            _provider.GetBytes(data);
            var randUint = BitConverter.ToUInt32(data, 0);
            return randUint / (uint.MaxValue + 1.0f);
        }

    }
}

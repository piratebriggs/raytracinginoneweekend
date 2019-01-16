using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace raytracinginoneweekend
{
    /// <summary>
    /// https://stackoverflow.com/a/16387724/6410376
    /// </summary>
    public class SunsetquestRandom : ImSoRandom
    {
        private static Random _provider = new Random();

        public float NextFloat()
        {
            lock (_provider)
            {
                return (float)_provider.Next() / ((float)int.MaxValue + 1.0f);
            }
        }
    }
}

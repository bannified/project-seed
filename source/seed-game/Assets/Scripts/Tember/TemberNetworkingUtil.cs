using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tember
{
    public static class Networking
    {
        public static string UIntToIP(uint value)
        {
            string res = "";
            uint mask = 0xFF000000;
            byte segment = (byte)(value & mask);
            res += segment;
            for (int i = 0; i < 3; i++)
            {
                value = value << 16;
                segment = (byte)(value & mask);
                res += segment + '.';
            }
            return res;
        }
    }
}


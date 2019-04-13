using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FH4TelemetryServer.DataStructs
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3 ()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }

        public Vector3 (int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 (float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}

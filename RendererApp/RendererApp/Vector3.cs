using System;
using System.Collections.Generic;
using System.Text;

namespace RendererApp
{
    public class Vector3
    {
        public float X;
        public float Y;
        public float Z;
        public int normalIndex;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float x, float y, float z, int normalIndex)
        {
            X = x;
            Y = y;
            Z = z;
            this.normalIndex = normalIndex;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3 Norm()
        {
            var length = Length;
            return new Vector3(X / length, Y / length, Z / length);
        }

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3 operator *(float num, Vector3 v)
        {
            return new Vector3(v.X * num, v.Y * num, v.Z * num);
        }

        public Vector3 CrossProduct(Vector3 edge2)
        {
            var u = this;
            var v = edge2;
            return new Vector3(
                u.Y * v.Z - u.Z * v.Y,
                u.Z * v.X - u.X * v.Z,
                u.X * v.Y - u.Y * v.X);
        }

        public float DotProduct(Vector3 other)
        {
            return this.X * other.X + this.Y * other.Y + this.Z * other.Z;
        }

        public static Vector3 Middle(Vector3 A, Vector3 B)
        {
            var sum = A + B;
            var middle = 0.5f * sum;
            return middle;
        }

        public bool IsInside(Vector3 A, Vector3 C1)
        {
            if (A.X > X || C1.X < X)
            {
                return false;
            }

            if (A.Y > Y || C1.Y < Y)
            {
                return false;
            }

            if (A.Z > Z || C1.Z < Z)
            {
                return false;
            }

            return true;
        }
    }
}

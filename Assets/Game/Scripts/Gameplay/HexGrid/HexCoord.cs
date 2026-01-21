using System;
using UnityEngine;

namespace Game.Gameplay.HexGrid
{
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        public readonly int Q;
        public readonly int R;

        public int S => -Q - R;

        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        public static readonly HexCoord[] Directions = new HexCoord[]
        {
            new(1, 0),   // Восток
            new(1, -1),  // Северо-восток
            new(0, -1),  // Северо-запад
            new(-1, 0),  // Запад
            new(-1, 1),  // Юго-запад
            new(0, 1)    // Юго-восток
        };

        public HexCoord GetNeighbor(int direction)
        {
            var dir = Directions[direction % 6];
            return new HexCoord(Q + dir.Q, R + dir.R);
        }

        public HexCoord[] GetAllNeighbors()
        {
            var neighbors = new HexCoord[6];
            for (int i = 0; i < 6; i++)
            {
                neighbors[i] = GetNeighbor(i);
            }
            return neighbors;
        }

        public int DistanceTo(HexCoord other)
        {
            return (Mathf.Abs(Q - other.Q) + Mathf.Abs(Q + R - other.Q - other.R) + Mathf.Abs(R - other.R)) / 2;
        }

        public Vector3 ToWorldPosition(float size)
        {
            float x = size * (Mathf.Sqrt(3f) * Q + Mathf.Sqrt(3f) / 2f * R);
            float z = size * (1.5f * R);
            return new Vector3(x, 0, z);
        }

        public static HexCoord FromWorldPosition(Vector3 worldPos, float size)
        {
            float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.z) / size;
            float r = (2f / 3f * worldPos.z) / size;
            return Round(q, r);
        }

        public static HexCoord Round(float q, float r)
        {
            float s = -q - r;
            
            int qi = Mathf.RoundToInt(q);
            int ri = Mathf.RoundToInt(r);
            int si = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(qi - q);
            float rDiff = Mathf.Abs(ri - r);
            float sDiff = Mathf.Abs(si - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                qi = -ri - si;
            }
            else if (rDiff > sDiff)
            {
                ri = -qi - si;
            }

            return new HexCoord(qi, ri);
        }

        public static HexCoord Zero => new(0, 0);

        public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);
        public static HexCoord operator -(HexCoord a, HexCoord b) => new(a.Q - b.Q, a.R - b.R);
        public static HexCoord operator *(HexCoord a, int scale) => new(a.Q * scale, a.R * scale);
        public static bool operator ==(HexCoord a, HexCoord b) => a.Q == b.Q && a.R == b.R;
        public static bool operator !=(HexCoord a, HexCoord b) => !(a == b);

        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;
        public override bool Equals(object obj) => obj is HexCoord coord && Equals(coord);
        public override int GetHashCode() => HashCode.Combine(Q, R);
        public override string ToString() => $"HexCoord({Q}, {R})";
    }
}


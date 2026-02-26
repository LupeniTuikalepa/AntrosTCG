using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATCG.HexGrids
{
    public readonly struct FractionalHexCoordinates
    {
        public readonly float q;
        public readonly float r;
        public readonly float s;

        public FractionalHexCoordinates(float q, float r, float s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (Mathf.Round(q + r + s) != 0)
                throw new ArgumentException("q + r + s must be 0");
        }

        public HexCoordinates HexRound()
        {
            int qi = (int)(Mathf.Round(q));
            int ri = (int)(Mathf.Round(r));
            int si = (int)(Mathf.Round(s));
            float q_diff = Mathf.Abs(qi - q);
            float r_diff = Mathf.Abs(ri - r);
            float s_diff = Mathf.Abs(si - s);

            if (q_diff > r_diff && q_diff > s_diff)
                qi = -ri - si;
            else if (r_diff > s_diff)
                ri = -qi - si;
            else
                si = -qi - ri;

            return new HexCoordinates(qi, ri);
        }


        public FractionalHexCoordinates HexLerp(FractionalHexCoordinates b, float t)
        {
            float x = q * (1 - t) + b.q * t;
            float y = r * (1 - t) + b.r * t;
            float z = s * (1 - t) + b.s * t;

            return new FractionalHexCoordinates(x, y, z);
        }
    }
}
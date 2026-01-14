using UnityEngine;

namespace TripleDots
{
    public static class HexMetrics
    {
        public static float OuterRadius(float hexSize)
        {
            return hexSize;
        }

        public static float InnerRadius(float hexSize)
        {
            return hexSize * 0.866025404f;
        }

        public static Vector3[] Corners(float hexSize, HexOrientation orientation)
        {
            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                corners[i] = Corner(hexSize, orientation, i);
            }

            return corners;
        }

        public static Vector3 Corner(float hexSize, HexOrientation orientation, int index)
        {
            var angle = 60f * index;
            if (orientation == HexOrientation.PointyTop)
            {
                angle += 30f;
            }

            var corner = new Vector3(hexSize * Mathf.Cos(angle * Mathf.Deg2Rad),
                0f,
                hexSize * Mathf.Sin(angle * Mathf.Deg2Rad));

            return corner;
        }
        
        public static Vector3 Center(float hexSize, int x, int z, HexOrientation orientation)
        {
            Vector3 centerPosition;
            if (orientation == HexOrientation.PointyTop)
            {
                float offset = (z % 2) * 0.5f;
                centerPosition.x = (x + offset) * (InnerRadius(hexSize) * 2f);
                centerPosition.y = 0f;
                centerPosition.z = z * (OuterRadius(hexSize) * 1.5f);
            }
            else
            {
                float offset = (x % 2) * 0.5f;
                centerPosition.x = x * (OuterRadius(hexSize) * 1.5f);
                centerPosition.y = 0f;
                centerPosition.z = (z + offset) * (InnerRadius(hexSize) * 2f);
            }
            
            return centerPosition;
        }
    }
}
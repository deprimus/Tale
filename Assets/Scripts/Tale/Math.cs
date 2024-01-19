using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaleUtil
{
    public static class Math
    {
        // Maps a value N from [min, max] to [newMin, newMax]
        public static float Map(float n, float min, float max, float newMin, float newMax)
        {
            return ((n - min) / (max - min)) * (newMax - newMin) + newMin;
        }

        // Maps a value N from [min, max] to [newMin, newMax] in an exponential manner.
        // Ease In: lambda > 1 starts slow and accelerates towards newMax
        // Ease Out: lambda < 1 starts fast and slowly converges to newMax
        // Linear: lambda can't be 1; use the normal Map() function for that case
        public static float ExpMap(float lambda, float n, float min, float max, float newMin, float newMax)
        {
            return (Mathf.Pow(lambda, n) - Mathf.Pow(lambda, min)) / (Mathf.Pow(lambda, max) - Mathf.Pow(lambda, min)) * (newMax - newMin) + newMin;
        }

        // Normalize an angle from any number to 0->360
        public static float NormalizeAngle(float angle)
        {
            if (angle != float.MinValue)
            {
                if (Mathf.Abs(angle) > 360f)
                {
                    angle %= 360;
                }

                if (angle < 0)
                {
                    angle += 360f;
                }
            }

            return angle;
        }

        // Given an angle, gets the nearest equivalent angle considering a target value
        // Example:
        // angle: 0, tagert: 270 -> angle: 360
        // angle: 0, target: 90  -> angle: 0
        public static float NearestEquivalentAngle(float angle, float target)
        {
            float diff = angle - target;

            if (Mathf.Abs(diff) > Mathf.Abs(diff + 360f))
            {
                return angle + 360f;
            }

            return angle;
        }
        
        public static float Identity(float t)
        {
            return t;
        }

        public static float QuadraticIn(float t)
        {
            return t * t;
        }

        public static float QuadraticOut(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        public static float ParametricBlend(float t)
        {
            float square = t * t;

            return square / (2f * (square - t) + 1f);
        }

        public static float Interpolate(float start, float end, float t)
        {
            t = Mathf.Clamp01(t);
            return start - (start - end) * t;
        }

        public static Color Interpolate(Color start, Color end, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(start.r - (start.r - end.r) * t, start.g - (start.g - end.g) * t, start.b - (start.b - end.b) * t, start.a - (start.a - end.a) * t);
        }

        public static Vector3 Interpolate(Vector3 start, Vector3 end, float t)
        {
            t = Mathf.Clamp01(t);
            return new Vector3(start.x - (start.x - end.x) * t, start.y - (start.y - end.y) * t, start.z - (start.z - end.z) * t);
        }
    }
}
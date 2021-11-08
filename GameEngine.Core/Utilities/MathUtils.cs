using System;
using System.Runtime.CompilerServices;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for mathematic operations
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Clamps the given float value between the given minimum and maximum values
        /// </summary>
        /// <param name="value">The float value to restrict inside the chosen range</param>
        /// <param name="min">The minimum float value to compare against</param>
        /// <param name="max">The maximum float value to compare against</param>
        /// <returns>The float that is closest to the given value within the min and max range</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max) => Math.Min(max, Math.Max(min, value));

        /// <summary>
        /// Clamps the given double value between the given minimum and maximum values
        /// </summary>
        /// <param name="value">The double value to restrict inside the chosen range</param>
        /// <param name="min">The minimum double value to compare against</param>
        /// <param name="max">The maximum double value to compare against</param>
        /// <returns>The double that is closest to the given value within the min and max range</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max) => Math.Min(max, Math.Max(min, value));

        /// <summary>
        /// Linearly interpolates between a start and an end value by a given float value 
        /// </summary>
        /// <param name="value">The interpolation value</param>
        /// <param name="start">The start float value to compare against</param>
        /// <param name="end">The end float value to compare against</param>
        /// <param name="clamp">If the interpolation value should be clamped between 0 and 1</param>
        /// <returns>The float result of the interpolation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float value, float start, float end, bool clamp = true) => (clamp ? Clamp(value, 0f, 1f) : value - start) / (end - start);

        /// <summary>
        /// Linearly interpolates between a start and an end value by a given double value 
        /// </summary>
        /// <param name="value">The interpolation value</param>
        /// <param name="start">The start double value to compare against</param>
        /// <param name="end">The end double value to compare against</param>
        /// <param name="clamp">If the interpolation value should be clamped between 0 and 1</param>
        /// <returns>The double result of the interpolation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double value, double start, double end, bool clamp = true) => (clamp ? Clamp(value, 0f, 1f) : value - start) / (end - start);
    }
}

using UnityEngine;

namespace GameEngine.Core.Unity.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for transform displacements operations
    /// </summary>
    public static class DisplacementUtils
    {
        /// <summary>
        /// Move and rotate the given transform in the direction of the target position and rotation with a kown speed in a known time interval
        /// </summary>
        /// <param name="transform">The transform to move</param>
        /// <param name="targetPosition">The target position to reach</param>
        /// <param name="targetRotation">The target rotation to reach</param>
        /// <param name="speed">The translation speed</param>
        /// <param name="deltaTime">The time interval</param>
        /// <returns>True if the target has been reached, otherwise False</returns>
        public static bool MoveTowards(this Transform transform, Vector3 targetPosition, Quaternion targetRotation, float speed, float deltaTime)
        {
            Vector3 remainingMove = targetPosition - transform.position;
            Vector3 movingDirection = remainingMove.normalized;
            float movingDistance = speed * deltaTime;

            if (movingDistance < remainingMove.magnitude)
            {
                float progressionRatio = movingDistance / remainingMove.magnitude;

                transform.position += movingDistance * movingDirection;
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, progressionRatio);
                return false;
            }
            else
            {
                transform.position = targetPosition;
                transform.localRotation = targetRotation;
                return true;
            }
        }
    }
}

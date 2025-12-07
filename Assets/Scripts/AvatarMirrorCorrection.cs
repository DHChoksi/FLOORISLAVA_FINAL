using UnityEngine;

namespace MultiplayerMeta.Scripts
{
    /// <summary>
    /// a utility script to correct avatar mirroring issues by inverting local position/rotation axes.
    /// Attach this to the Avatar object that is being moved by the NetworkTransform.
    /// </summary>
    public class AvatarMirrorCorrection : MonoBehaviour
    {
        [Header("Position Correction")]
        [Tooltip("Invert the X coordinate of the local position.")]
        public bool invertXPosition = false;

        [Tooltip("Invert the Y coordinate of the local position.")]
        public bool invertYPosition = false;

        [Tooltip("Invert the Z coordinate of the local position.")]
        public bool invertZPosition = false;

        [Header("Rotation Correction")]
        [Tooltip("Invert the Y (Yaw) rotation.")]
        public bool invertYRotation = false;

        [Tooltip("Add 180 degrees to the Y rotation. Useful if avatar is facing backwards.")]
        public bool offsetRotation180 = false;

        private void LateUpdate()
        {
            // Apply Position Corrections
            if (invertXPosition || invertYPosition || invertZPosition)
            {
                Vector3 pos = transform.localPosition;
                if (invertXPosition) pos.x = -pos.x;
                if (invertYPosition) pos.y = -pos.y;
                transform.localPosition = pos;
            }

            // Apply Rotation Corrections
            if (invertYRotation || offsetRotation180)
            {
                Vector3 euler = transform.localEulerAngles;
                
                if (invertYRotation)
                {
                    // Invert around 0 or relative? 
                    // Usually mirroring rotation means angle -> -angle
                    // But we must handle the 0-360 wrap correctly if doing math, 
                    // but negating euler.y usually works for simple mirroring.
                    euler.y = -euler.y; 
                }

                if (offsetRotation180)
                {
                    euler.y += 180f;
                }

                transform.localEulerAngles = euler;
            }
        }
    }
}

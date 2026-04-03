using Il2CppCraftEditor;
using UnityEngine;

namespace CEImprovements;

// prevent camera roll (tilting head left/right) without limiting pitch at all
// uses quaternion decomposition to avoid euler gimbal lock issues
internal class RotationClamper
{
    internal void Clamp(CECamera cam)
    {
        var t = cam.camera.transform;
        var rot = t.rotation;

        // project forward onto the horizontal plane to get yaw
        var forward = rot * Vector3.forward;
        var right = rot * Vector3.right;

        // check if there's any roll — dot of camera's right with world up
        float roll = Vector3.Dot(right, Vector3.up);
        // also check the actual up alignment
        var up = rot * Vector3.up;
        float upDot = Vector3.Dot(up, Vector3.Cross(forward, Vector3.Cross(Vector3.up, forward).normalized).normalized);

        // rebuild rotation from just forward direction (no roll)
        // LookRotation with world up naturally strips roll
        // but fails at poles, so use the camera's projected right instead
        var flatRight = Vector3.Cross(Vector3.up, forward);
        if (flatRight.sqrMagnitude < 0.001f)
        {
            // looking straight up or down — use current right projected to horizontal
            flatRight = new Vector3(right.x, 0f, right.z).normalized;
        }
        else
        {
            flatRight = flatRight.normalized;
        }

        var noRollUp = Vector3.Cross(forward, flatRight).normalized;
        var noRollRot = Quaternion.LookRotation(forward, noRollUp);

        if (Quaternion.Angle(rot, noRollRot) > 0.05f)
        {
            t.rotation = noRollRot;
            t.position = cam.pivot - t.forward * cam.zoom;
        }
    }
}

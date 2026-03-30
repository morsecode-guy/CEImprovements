using Il2CppCraftEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CEImprovements;

// wasd + shift+mouse fps camera — puts the pivot right on the camera
// so the game's orbit becomes our free look
internal class FpsCameraController
{
    const float BaseSpeed = 3.3f;
    const float ShiftMultiplier = 3f;
    const float MoveSmoothTime = 0.12f;

    Vector3 _velocity;
    Vector3 _smoothDampVel;
    Vector3 _fpsCamPos;
    float _fpsExitTimer;
    float _savedZoom; // restore this when leaving fps mode

    internal bool InFpsMode { get; private set; }

    // check if we should enter or stay in fps mode
    internal void CheckEnter(CECamera cam, Keyboard kb)
    {
        bool anyWasd = kb.wKey.isPressed || kb.sKey.isPressed ||
                       kb.aKey.isPressed || kb.dKey.isPressed ||
                       kb.eKey.isPressed || kb.qKey.isPressed;

        var mouse = Mouse.current;
        bool shiftHeld = kb.shiftKey.isPressed;
        // middle mouse is the game's orbit, dont fight it
        bool mouseHeld = mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed);
        bool shiftMouse = shiftHeld && mouseHeld;

        bool wantsFps = anyWasd || shiftMouse;

        if (wantsFps)
        {
            _fpsExitTimer = 0.3f;

            if (!InFpsMode)
            {
                _fpsCamPos = cam.camera.transform.position;
                _savedZoom = cam.zoom;
                InFpsMode = true;
            }

            // keep pivot on camera so orbit = free look
            cam.pivot = _fpsCamPos;
            cam.zoom = 0.001f;
            cam.zoomVel = 0f;
        }
        else if (InFpsMode)
        {
            // still in fps but no input — keep camera pinned until grace timer runs out
            cam.pivot = _fpsCamPos;
            cam.zoom = 0.001f;
            cam.zoomVel = 0f;
        }
    }

    // move camera with wasd/eq, smooth it out so it feels nice
    internal void UpdateMovement(CECamera cam, Keyboard kb, ZoomController zoom)
    {
        if (!InFpsMode) return;

        var camTransform = cam.camera.transform;

        float moveX = 0f, moveZ = 0f, moveY = 0f;
        if (kb.wKey.isPressed) moveZ += 1f;
        if (kb.sKey.isPressed) moveZ -= 1f;
        if (kb.dKey.isPressed) moveX += 1f;
        if (kb.aKey.isPressed) moveX -= 1f;
        if (kb.eKey.isPressed) moveY += 1f; // up
        if (kb.qKey.isPressed) moveY -= 1f; // down

        bool isMoving = moveX != 0f || moveY != 0f || moveZ != 0f;

        if (isMoving)
        {
            float speed = BaseSpeed;
            if (kb.shiftKey.isPressed)
                speed *= ShiftMultiplier;

            // flatten forward/right so wasd moves on the horizontal plane
            var forward = camTransform.forward;
            var right = camTransform.right;

            forward.y = 0f;
            forward = forward.normalized;
            right.y = 0f;
            right = right.normalized;

            var targetVelocity = (right * moveX + forward * moveZ + Vector3.up * moveY) * speed;
            _velocity = Vector3.SmoothDamp(_velocity, targetVelocity, ref _smoothDampVel, MoveSmoothTime);
        }
        else
        {
            _velocity = Vector3.SmoothDamp(_velocity, Vector3.zero, ref _smoothDampVel, MoveSmoothTime);
        }

        _fpsCamPos += _velocity * Time.deltaTime;
        camTransform.position = _fpsCamPos;
        cam.pivot = _fpsCamPos;
        cam.zoom = 0.001f;

        // grace period so you dont drop out of fps between key taps
        if (!isMoving && !kb.shiftKey.isPressed)
        {
            _fpsExitTimer -= Time.deltaTime;
            if (_fpsExitTimer <= 0f && _velocity.sqrMagnitude < 0.01f)
                Exit(cam, zoom);
        }
    }

    // put the pivot back in front of the camera and restore zoom
    internal void Exit(CECamera cam, ZoomController zoom)
    {
        if (!InFpsMode) return;

        var camTransform = cam.camera.transform;
        float restoreZoom = _savedZoom > 1f ? _savedZoom : 15f;
        cam.pivot = _fpsCamPos + camTransform.forward * restoreZoom;
        cam.zoom = restoreZoom;
        zoom.Reset(restoreZoom);
        InFpsMode = false;
        _velocity = Vector3.zero;
        _smoothDampVel = Vector3.zero;
    }
}

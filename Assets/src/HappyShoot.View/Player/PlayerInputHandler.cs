using UnityEngine;
using UnityEngine.InputSystem;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Captures player input using Unity modern Input System package
    /// and relays movement and dash commands to PlayerView and PlayerDashController.
    /// Supports Mobile Floating Touch Joystick, Hardware Keyboard (Space to Dash), and Gamepad.
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerView _playerView;
        private PlayerDashController _dashController;
        private UI.TouchJoystickView _touchJoystick;

        private void Awake()
        {
            _playerView = GetComponent<PlayerView>();
            _dashController = GetComponent<PlayerDashController>();
        }

        public void SetTouchJoystick(UI.TouchJoystickView joystick)
        {
            _touchJoystick = joystick;
        }

        public void SetDashController(PlayerDashController dashController)
        {
            _dashController = dashController;
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            float horizontal = 0f;
            float vertical = 0f;

            // 1. Mobile Virtual Touch Joystick Input
            if (_touchJoystick != null && _touchJoystick.InputVector.sqrMagnitude > 0.001f)
            {
                horizontal = _touchJoystick.InputVector.x;
                vertical = _touchJoystick.InputVector.y;
            }

            // 2. Hardware Keyboard Input (Hybrid fallback / PC testing)
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            }

            // 3. Hardware Gamepad Input
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f)
                {
                    horizontal = stick.x;
                    vertical = stick.y;
                }
            }

            // 4. Spacebar / Gamepad Dash Trigger
            bool dashTriggered = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                || (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame));

            if (dashTriggered && _dashController != null)
            {
                Vector2 inputDir = new Vector2(horizontal, vertical);
                _dashController.TryDash(inputDir);
            }

            // 5. While dashing, physics overrides standard walking movement
            if (_dashController != null && _dashController.IsDashing)
                return;

            // Standard walk movement
            if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
            {
                Vector2D direction = new Vector2D(horizontal, vertical);
                _playerView.Entity.Move(direction, Time.deltaTime);
            }
        }
    }
}

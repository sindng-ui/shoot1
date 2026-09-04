using UnityEngine;
using UnityEngine.InputSystem;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Captures player input using Unity modern Input System package
    /// and relays movement and dash commands to PlayerView and PlayerDashController.
    /// Supports Mobile Floating Touch Joystick, Hardware Keyboard (Space to Dash, W/Up to Jump in Side-Scroll), and Gamepad.
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerView _playerView;
        private PlayerDashController _dashController;
        private UI.TouchJoystickView _touchJoystick;

        // Side-Scrolling Jump Physics
        private float _jumpVelocity = 0f;
        private bool _isGrounded = true;
        private float _lastGroundedY = -1.8f;
        private const float JumpSpeed = 12.0f;
        private const float Gravity = -28f;

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

        public bool IsSideScrollMode { get; set; }
        public float SideScrollFixedY { get; set; } = -1.8f;

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

            // 2. Hardware Keyboard Input
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

            // 4. Side-Scrolling Jump & Grounding Physics with Dynamic Variable Platforms
            float currentY = _playerView.Entity.Position.Y;
            float playerX = _playerView.transform.position.x;

            if (IsSideScrollMode)
            {
                vertical = 0f; // Disable standard 2D top-down vertical movement
                var platformMgr = SideScroll.SideScrollPlatformManager.Instance;

                // If platform manager is handling chasm fall animation, yield control
                if (platformMgr != null && platformMgr.IsFalling)
                {
                    return;
                }

                bool jumpPressed = (keyboard != null && (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame))
                    || (gamepad != null && gamepad.dpad.up.wasPressedThisFrame)
                    || (_touchJoystick != null && _touchJoystick.InputVector.y > 0.65f && _isGrounded);

                if (jumpPressed && _isGrounded)
                {
                    _jumpVelocity = JumpSpeed;
                    _isGrounded = false;
                    _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.WindGlaiveHit));
                }

                if (!_isGrounded)
                {
                    float prevY = currentY;
                    _jumpVelocity += Gravity * Time.deltaTime;
                    currentY += _jumpVelocity * Time.deltaTime;

                    // Landing check while descending (continuous vertical sweep to prevent premature drop)
                    if (_jumpVelocity <= 0f && platformMgr != null && platformMgr.TryGetPlatformLanding(playerX, prevY, currentY, out float surfaceY))
                    {
                        currentY = surfaceY;
                        _jumpVelocity = 0f;
                        _isGrounded = true;
                        _lastGroundedY = surfaceY;
                        if (platformMgr.TryGetPlatformAtX(playerX, out _, out float centerLandingX))
                        {
                            platformMgr.RegisterSafePlatform(centerLandingX, surfaceY);
                        }
                    }
                    else if (currentY <= _lastGroundedY - 2.5f && platformMgr != null)
                    {
                        // Fallen 2.5m below last grounded position → chasm fall!
                        platformMgr.TriggerChasmFall();
                        return;
                    }
                }
                else
                {
                    // While grounded, keep snapped to current platform, or detect stepping into chasm gap
                    if (platformMgr != null)
                    {
                        if (platformMgr.TryGetPlatformAtX(playerX, out float surfaceY, out float platformCenterX))
                        {
                            currentY = surfaceY;
                            _lastGroundedY = surfaceY;
                            platformMgr.RegisterSafePlatform(platformCenterX, surfaceY);
                        }
                        else
                        {
                            // Stepped off the edge into a chasm!
                            _isGrounded = false;
                            _jumpVelocity = -2.0f;
                        }
                    }
                }
            }

            // 5. Spacebar / Gamepad Dash Trigger (Horizontal flash-step in side-scroller)
            bool dashTriggered = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                || (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame));

            if (dashTriggered && _dashController != null)
            {
                Vector2 inputDir = IsSideScrollMode
                    ? new Vector2(Mathf.Abs(horizontal) > 0.01f ? Mathf.Sign(horizontal) : (_playerView.LastAimDirection.x >= 0 ? 1f : -1f), 0f)
                    : new Vector2(horizontal, vertical);
                _dashController.TryDash(inputDir);
            }

            // 6. While dashing, physics overrides standard movement
            if (_dashController != null && _dashController.IsDashing)
            {
                if (IsSideScrollMode)
                {
                    _playerView.Entity.SetPosition(new Vector2D(_playerView.Entity.Position.X, currentY));
                    _playerView.transform.position = new Vector3((float)_playerView.Entity.Position.X, currentY, 0f);
                }
                return;
            }

            // 7. Standard walk movement
            if (Mathf.Abs(horizontal) > 0.01f || (!IsSideScrollMode && Mathf.Abs(vertical) > 0.01f))
            {
                Vector2D direction = new Vector2D(horizontal, IsSideScrollMode ? 0f : vertical);
                _playerView.Entity.Move(direction, Time.deltaTime);
            }

            if (IsSideScrollMode)
            {
                _playerView.Entity.SetPosition(new Vector2D(_playerView.Entity.Position.X, currentY));
                _playerView.transform.position = new Vector3((float)_playerView.Entity.Position.X, currentY, 0f);
            }
        }

        /// <summary>
        /// Resets grounded status and vertical jump velocity when respawned or placed on a platform.
        /// </summary>
        public void ResetGroundedState(float surfaceY)
        {
            _isGrounded = true;
            _jumpVelocity = 0f;
            _lastGroundedY = surfaceY;
            if (_playerView?.Entity != null)
            {
                _playerView.Entity.SetPosition(new Vector2D(_playerView.Entity.Position.X, surfaceY));
                _playerView.transform.position = new Vector3((float)_playerView.Entity.Position.X, surfaceY, 0f);
            }
        }
    }
}

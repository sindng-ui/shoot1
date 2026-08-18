using UnityEngine;
using UnityEngine.InputSystem;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Captures player input using Unity's modern Input System package
    /// and relays movement commands to the PlayerView's domain entity.
    /// </summary>
    [RequireComponent(typeof(PlayerView))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerView _playerView;

        private void Awake()
        {
            _playerView = GetComponent<PlayerView>();
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            float horizontal = 0f;
            float vertical = 0f;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
            }

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

            if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
            {
                Vector2D direction = new Vector2D(horizontal, vertical);
                _playerView.Entity.Move(direction, Time.deltaTime);
            }
        }
    }
}

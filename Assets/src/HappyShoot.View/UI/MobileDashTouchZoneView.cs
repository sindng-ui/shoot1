using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using HappyShoot.View.Player;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Invisible mobile touch zone covering the right half of the screen (55%~100%).
    /// Tapping anywhere on the right half triggers player Dash immediately in current joystick direction.
    /// Strictly modular, zero-allocation, and under 500 lines.
    /// </summary>
    public class MobileDashTouchZoneView : MonoBehaviour, IPointerDownHandler
    {
        private PlayerDashController _dashController;
        private TouchJoystickView _touchJoystick;

        public void Initialize(PlayerDashController dashController, TouchJoystickView touchJoystick)
        {
            _dashController = dashController;
            _touchJoystick = touchJoystick;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // On PC standalone with mouse, ignore left clicks so mouse aiming is uninterrupted
            if (!Application.isMobilePlatform && Touchscreen.current == null && eventData.pointerId == -1)
            {
                // Only allow right-click (button == 1) on PC mouse for testing, or ignore
                if (eventData.button != PointerEventData.InputButton.Right)
                    return;
            }

            TriggerMobileDash();
        }

        private void TriggerMobileDash()
        {
            if (_dashController == null) return;

            Vector2 moveDir = Vector2.zero;
            if (_touchJoystick != null && _touchJoystick.InputVector.sqrMagnitude > 0.01f)
            {
                moveDir = _touchJoystick.InputVector;
            }

            _dashController.TryDash(moveDir);
        }

        public static MobileDashTouchZoneView Create(Transform parent, Canvas canvas, PlayerDashController dashController, TouchJoystickView joystick)
        {
            var zoneGo = new GameObject("MobileDashTouchZone", typeof(RectTransform), typeof(Image));
            zoneGo.transform.SetParent(parent, false);

            var rt = zoneGo.GetComponent<RectTransform>();
            // Right 45% of the screen (0.55 to 1.0)
            rt.anchorMin = new Vector2(0.55f, 0f);
            rt.anchorMax = new Vector2(1.0f, 1.0f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = zoneGo.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // Completely invisible
            img.raycastTarget = true;

            var view = zoneGo.AddComponent<MobileDashTouchZoneView>();
            view.Initialize(dashController, joystick);

            return view;
        }
    }
}

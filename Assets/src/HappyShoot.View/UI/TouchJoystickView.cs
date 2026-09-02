using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Pure Floating Mobile Touch Joystick.
    /// Completely hidden when not touching. Pops up exactly at finger touch position,
    /// and disappears immediately when finger is released.
    /// On Windows/PC, stays completely invisible and does not intercept mouse clicks.
    /// </summary>
    public class TouchJoystickView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Configuration")]
        [SerializeField] private float _handleRange = 70f;
        [SerializeField] private float _deadZone = 0.08f;

        private Canvas _parentCanvas;
        private RectTransform _touchZoneRect;
        private RectTransform _baseRect;
        private RectTransform _knobRect;
        private Image _baseImage;
        private Image _knobImage;

        private int _activePointerId = -999;
        private bool _isDragging = false;
        private Vector2 _inputVector = Vector2.zero;

        public Vector2 InputVector => _inputVector;
        public bool IsDragging => _isDragging;

        public void Initialize(Canvas parentCanvas, RectTransform touchZoneRect, RectTransform baseRect, RectTransform knobRect)
        {
            _parentCanvas = parentCanvas;
            _touchZoneRect = touchZoneRect;
            _baseRect = baseRect;
            _knobRect = knobRect;

            _baseImage = _baseRect.GetComponent<Image>();
            _knobImage = _knobRect.GetComponent<Image>();

            // Completely hide on initialization
            HideJoystick();
        }

        private void ShowJoystickAt(Vector2 screenPosition)
        {
            Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_baseRect.parent, screenPosition, cam, out Vector2 localPoint))
            {
                _baseRect.anchoredPosition = localPoint;
            }

            _knobRect.anchoredPosition = Vector2.zero;
            _baseRect.gameObject.SetActive(true);

            if (_baseImage != null) _baseImage.color = new Color(1f, 1f, 1f, 0.85f);
            if (_knobImage != null) _knobImage.color = new Color(1f, 1f, 1f, 1.0f);
        }

        private void HideJoystick()
        {
            _isDragging = false;
            _activePointerId = -999;
            _inputVector = Vector2.zero;
            if (_knobRect != null) _knobRect.anchoredPosition = Vector2.zero;
            if (_baseRect != null) _baseRect.gameObject.SetActive(false);
        }

        #region EventSystem Callbacks (Touch Only, Ignore Mouse on PC)

        public void OnPointerDown(PointerEventData eventData)
        {
            // If running on non-mobile platform without touch, ignore mouse clicks
            // (Preserves PC mouse aim reticle)
            if (!Application.isMobilePlatform && eventData.pointerId >= 0 && Touchscreen.current == null)
            {
                return;
            }

            if (_isDragging) return;

            _activePointerId = eventData.pointerId;
            _isDragging = true;
            ShowJoystickAt(eventData.position);
            UpdateJoystickPosition(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || eventData.pointerId != _activePointerId) return;
            UpdateJoystickPosition(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId) return;
            HideJoystick();
        }

        #endregion

        #region Direct Hardware Touch Fallback (Mobile Devices)

        private void Update()
        {
            // On standalone Windows PC, do not poll touchscreen
            if (!Application.isMobilePlatform && Touchscreen.current == null && Input.touchCount == 0)
            {
                return;
            }

            if (_isDragging) return;

            // 1. New Input System Touchscreen
            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.isPressed)
                    {
                        Vector2 screenPos = touch.position.ReadValue();
                        if (IsInLeftZone(screenPos))
                        {
                            _activePointerId = touch.touchId.ReadValue();
                            _isDragging = true;
                            ShowJoystickAt(screenPos);
                            UpdateJoystickPosition(screenPos);
                            return;
                        }
                    }
                }
            }

            // 2. Legacy Touch Fallback
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (t.phase == UnityEngine.TouchPhase.Began || t.phase == UnityEngine.TouchPhase.Moved || t.phase == UnityEngine.TouchPhase.Stationary)
                    {
                        if (IsInLeftZone(t.position))
                        {
                            _activePointerId = t.fingerId;
                            _isDragging = true;
                            ShowJoystickAt(t.position);
                            UpdateJoystickPosition(t.position);
                            return;
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (!_isDragging) return;

            bool stillPressed = false;
            Vector2 currentScreenPos = Vector2.zero;

            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.isPressed && touch.touchId.ReadValue() == _activePointerId)
                    {
                        stillPressed = true;
                        currentScreenPos = touch.position.ReadValue();
                        break;
                    }
                }
            }

            if (!stillPressed && Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (t.fingerId == _activePointerId && t.phase != UnityEngine.TouchPhase.Ended && t.phase != UnityEngine.TouchPhase.Canceled)
                    {
                        stillPressed = true;
                        currentScreenPos = t.position;
                        break;
                    }
                }
            }

            if (stillPressed)
            {
                UpdateJoystickPosition(currentScreenPos);
            }
            else
            {
                // Touch was released, immediately hide!
                HideJoystick();
            }
        }

        private bool IsInLeftZone(Vector2 screenPos)
        {
            return screenPos.x <= Screen.width * 0.55f;
        }

        #endregion

        private void UpdateJoystickPosition(Vector2 screenPosition)
        {
            Camera cam = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _baseRect, screenPosition, cam, out Vector2 localPoint))
            {
                float distance = localPoint.magnitude;
                Vector2 direction = distance > 0.0001f ? localPoint / distance : Vector2.zero;

                float clampedDist = Mathf.Min(distance, _handleRange);
                _knobRect.anchoredPosition = direction * clampedDist;

                float normalizedDist = clampedDist / _handleRange;
                if (normalizedDist < _deadZone)
                {
                    _inputVector = Vector2.zero;
                }
                else
                {
                    float remapped = (normalizedDist - _deadZone) / (1f - _deadZone);
                    _inputVector = direction * Mathf.Clamp01(remapped);
                }
            }
        }

        private void OnDisable()
        {
            HideJoystick();
        }

        public static TouchJoystickView Create(Transform parent, Canvas canvas)
        {
            var rootGo = new GameObject("TouchJoystickRoot", typeof(RectTransform));
            rootGo.transform.SetParent(parent, false);

            var rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Touch zone: left 55% of the screen
            var zoneGo = new GameObject("TouchZone", typeof(RectTransform), typeof(Image));
            zoneGo.transform.SetParent(rootGo.transform, false);
            var zoneRt = zoneGo.GetComponent<RectTransform>();
            zoneRt.anchorMin = new Vector2(0f, 0f);
            zoneRt.anchorMax = new Vector2(0.55f, 1f);
            zoneRt.offsetMin = Vector2.zero;
            zoneRt.offsetMax = Vector2.zero;

            var zoneImg = zoneGo.GetComponent<Image>();
            zoneImg.color = new Color(0f, 0f, 0f, 0f);
            zoneImg.raycastTarget = true;

            // Base Ring (160x160) - Initially hidden
            var baseGo = new GameObject("JoystickBase", typeof(RectTransform), typeof(Image));
            baseGo.transform.SetParent(rootGo.transform, false);
            var baseRt = baseGo.GetComponent<RectTransform>();
            baseRt.sizeDelta = new Vector2(160f, 160f);
            baseRt.pivot = new Vector2(0.5f, 0.5f);

            var baseImg = baseGo.GetComponent<Image>();
            baseImg.sprite = TouchJoystickSpriteHelper.GetOrCreateBaseSprite();
            baseImg.color = new Color(1f, 1f, 1f, 0.85f);
            baseImg.raycastTarget = false;
            baseGo.SetActive(false); // Initially hidden!

            // Knob (72x72)
            var knobGo = new GameObject("JoystickKnob", typeof(RectTransform), typeof(Image));
            knobGo.transform.SetParent(baseGo.transform, false);
            var knobRt = knobGo.GetComponent<RectTransform>();
            knobRt.sizeDelta = new Vector2(72f, 72f);
            knobRt.anchoredPosition = Vector2.zero;
            knobRt.pivot = new Vector2(0.5f, 0.5f);

            var knobImg = knobGo.GetComponent<Image>();
            knobImg.sprite = TouchJoystickSpriteHelper.GetOrCreateKnobSprite();
            knobImg.color = Color.white;
            knobImg.raycastTarget = false;

            var joystickView = zoneGo.AddComponent<TouchJoystickView>();
            joystickView.Initialize(canvas, zoneRt, baseRt, knobRt);

            return joystickView;
        }
    }
}

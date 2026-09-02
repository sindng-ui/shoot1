using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Session;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Touch-friendly mobile pause button positioned at top-right of the screen.
    /// Provides one-tap access to PauseMenuUiView on touch devices where ESC key is unavailable.
    /// </summary>
    public class MobilePauseButtonView : MonoBehaviour
    {
        private GameSessionEntity _gameSession;
        private EventBus _eventBus;
        private Button _button;
        private GameObject _rootObject;

        public void Initialize(GameSessionEntity gameSession, EventBus eventBus)
        {
            _gameSession = gameSession;
            _eventBus = eventBus;

            if (_eventBus != null)
            {
                _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            }
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            }
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (_rootObject != null)
            {
                // Only visible during active gameplay
                _rootObject.SetActive(evt.NewState == GameState.Playing);
            }
        }

        public void OnPauseButtonClicked()
        {
            if (_gameSession != null && _gameSession.IsPlaying)
            {
                _gameSession.Pause();
            }
        }

        /// <summary>
        /// Programmatically creates the mobile pause button on the HUD Canvas.
        /// </summary>
        public static MobilePauseButtonView Create(Transform parentCanvasTransform, GameSessionEntity session, EventBus bus)
        {
            var btnGo = new GameObject("MobilePauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parentCanvasTransform, false);

            var rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-28f, -24f);
            rt.sizeDelta = new Vector2(56f, 56f);

            var img = btnGo.GetComponent<Image>();
            img.sprite = CreatePauseIconSprite();
            img.color = Color.white;
            img.raycastTarget = true;

            var btn = btnGo.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.85f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.6f, 0.85f, 1f, 1f);
            btn.colors = colors;

            var view = btnGo.AddComponent<MobilePauseButtonView>();
            view._rootObject = btnGo;
            view._button = btn;
            view.Initialize(session, bus);

            btn.onClick.AddListener(view.OnPauseButtonClicked);

            return view;
        }

        private static Sprite CreatePauseIconSprite()
        {
            int size = 56;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color transparent = new Color(0f, 0f, 0f, 0f);
            Color bgColor = new Color(0.06f, 0.12f, 0.22f, 0.75f); // Dark translucent frame
            Color rimColor = new Color(0.3f, 0.75f, 1.0f, 0.85f);  // Cyan border
            Color barColor = new Color(0.9f, 0.96f, 1.0f, 0.95f);  // Crisp white bars

            float center = (size - 1) * 0.5f;
            float radius = center - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > radius)
                    {
                        pixels[y * size + x] = transparent;
                    }
                    else if (dist > radius - 2.5f)
                    {
                        pixels[y * size + x] = rimColor;
                    }
                    else
                    {
                        // Draw two vertical pause bars: x in [18..23] and [32..37], y in [16..39]
                        bool isLeftBar = (x >= 19 && x <= 24 && y >= 16 && y <= 39);
                        bool isRightBar = (x >= 31 && x <= 36 && y >= 16 && y <= 39);

                        pixels[y * size + x] = (isLeftBar || isRightBar) ? barColor : bgColor;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

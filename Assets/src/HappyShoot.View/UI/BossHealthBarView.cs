using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Master Boss Health Bar displayed prominently at the top of the screen during boss battles.
    /// Supports automatic procedural UI creation and smooth animated filling.
    /// </summary>
    public class BossHealthBarView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Text _bossNameText;
        [SerializeField] private Text _healthText;

        private float _targetFill;
        private float _currentFill;
        private EventBus _eventBus;
        private readonly StringBuilder _sb = new StringBuilder(32);

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            EnsureUiElements();

            if (_eventBus != null)
            {
                _eventBus.Subscribe<BossSpawnedEvent>(OnBossSpawned);
                _eventBus.Subscribe<BossHealthUpdatedEvent>(OnBossHealthUpdated);
                _eventBus.Subscribe<BossDiedEvent>(OnBossDied);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (_healthSlider != null && Mathf.Abs(_currentFill - _targetFill) > 0.001f)
            {
                _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.unscaledDeltaTime * 8f);
                _healthSlider.value = _currentFill;
            }
        }

        private void OnBossSpawned(BossSpawnedEvent evt)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            if (_bossNameText != null)
            {
                _bossNameText.text = $"💀 {evt.BossName.ToUpper()} 💀";
            }

            _targetFill = 1f;
            _currentFill = 1f;
            if (_healthSlider != null) _healthSlider.value = 1f;

            UpdateHpText(evt.MaxHealth, evt.MaxHealth);
        }

        private void OnBossHealthUpdated(BossHealthUpdatedEvent evt)
        {
            float fill = evt.MaxHealth > 0f ? Mathf.Clamp01(evt.CurrentHealth / evt.MaxHealth) : 0f;
            _targetFill = fill;
            UpdateHpText(evt.CurrentHealth, evt.MaxHealth);
        }

        private void OnBossDied(BossDiedEvent evt)
        {
            _targetFill = 0f;
            Invoke(nameof(HideBossBar), 1.5f);
        }

        private void HideBossBar()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void UpdateHpText(float current, float max)
        {
            if (_healthText != null)
            {
                _sb.Clear();
                _sb.Append((int)current).Append(" / ").Append((int)max);
                _healthText.text = _sb.ToString();
            }
        }

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("BossBarCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panelRoot = new GameObject("BossBarRoot");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var rootRt = _panelRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 1f);
            rootRt.anchorMax = new Vector2(0.5f, 1f);
            rootRt.pivot = new Vector2(0.5f, 1f);
            rootRt.anchoredPosition = new Vector2(0f, -80f);
            rootRt.sizeDelta = new Vector2(520f, 60f);

            // Boss Name Text
            _bossNameText = CreateText(_panelRoot.transform, "BossName", "💀 BOSS NAME 💀", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(400f, 26f), new Color(1f, 0.3f, 0.35f, 1f));

            // Bar Background
            var bgGo = CreateUiPanel(_panelRoot.transform, "BossHpBg", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(480f, 26f), new Color(0.12f, 0.05f, 0.08f, 0.9f));
            var fillGo = CreateUiPanel(bgGo.transform, "BossHpFill", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.9f, 0.15f, 0.25f, 1f));

            _healthSlider = bgGo.AddComponent<Slider>();
            _healthSlider.targetGraphic = bgGo.GetComponent<Image>();
            _healthSlider.fillRect = fillGo.GetComponent<RectTransform>();
            _healthSlider.minValue = 0f;
            _healthSlider.maxValue = 1f;
            _healthSlider.value = 1f;

            _healthText = CreateText(bgGo.transform, "HpText", "1000 / 1000", 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 20f), Color.white);
        }

        private GameObject CreateUiPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private Text CreateText(Transform parent, string name, string defaultText, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var txt = go.AddComponent<Text>();
            txt.text = defaultText;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.UI;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Lightweight, high-impact 3D TextMesh view for floating damage numbers.
    /// Features heavy black stroke outline shader, vibrant elemental color palettes (Fire, Ice, Lightning, White),
    /// zero-allocation string formatting, and a dynamic -12° diagonal tilt on critical strikes.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class DamageTextView : MonoBehaviour
    {
        private DamageTextEntity _entity;
        private TextMesh _textMesh;
        private MeshRenderer _meshRenderer;
        private static Material _sharedOutlineMaterial;
        private static Font _sharedFont;

        private float _spawnTimer = 0f;
        private float _targetBaseScale = 0.115f;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            _textMesh = GetComponent<TextMesh>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _textMesh.alignment = TextAlignment.Center;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.characterSize = 1f;

            if (_meshRenderer != null)
            {
                // Ensure floating numbers render in front of monsters, projectiles, and ground
                _meshRenderer.sortingOrder = 350;
            }

            InitMaterialAndFont();
        }

        private void InitMaterialAndFont()
        {
            if (_sharedFont == null)
            {
                _sharedFont = DamageFontHelper.GetDamageFont();
            }

            if (_sharedFont != null)
            {
                _textMesh.font = _sharedFont;

                if (_sharedOutlineMaterial == null)
                {
                    Shader outlineShader = Shader.Find("HappyShoot/DamageTextOutline");
                    if (outlineShader != null)
                    {
                        _sharedOutlineMaterial = new Material(outlineShader);
                        if (_sharedFont.material != null)
                        {
                            _sharedOutlineMaterial.mainTexture = _sharedFont.material.mainTexture;
                        }
                    }
                    else if (_sharedFont.material != null)
                    {
                        _sharedOutlineMaterial = _sharedFont.material;
                    }
                }

                if (_meshRenderer != null && _sharedOutlineMaterial != null)
                {
                    _meshRenderer.sharedMaterial = _sharedOutlineMaterial;
                }
            }
        }

        public void Bind(DamageTextEntity entity)
        {
            _entity = entity;
            _spawnTimer = 0f;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, -2.5f);

            // Zero-allocation formatted text retrieval
            _textMesh.text = DamageNumberCache.GetString(entity.DamageValue, entity.IsCritical);
            _baseColor = DamageColorPalette.GetColor(entity.DamageType, entity.IsCritical);
            _textMesh.color = _baseColor;
            _textMesh.fontStyle = FontStyle.Bold;

            if (entity.IsCritical)
            {
                _textMesh.fontSize = 48;
                _targetBaseScale = 0.165f; // Extra large for critical strikes

                // Diagonal tilt: -12 degrees for dynamic comic/RPG feel (0% runtime per-frame overhead!)
                transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
                transform.localScale = Vector3.one * (_targetBaseScale * 1.45f);
            }
            else
            {
                _textMesh.fontSize = 38;
                _targetBaseScale = 0.115f; // Highly visible on mobile screens

                // Upright for standard strikes
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one * _targetBaseScale;
            }

            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive)
            {
                gameObject.SetActive(false);
                return;
            }

            _spawnTimer += Time.deltaTime;

            if (_entity.IsCritical)
            {
                // Dynamic punch bounce: 1.45x -> 1.0x within 0.1s
                float pop = Mathf.Lerp(1.45f, 1.0f, Mathf.Clamp01(_spawnTimer * 10f));
                transform.localScale = Vector3.one * (_targetBaseScale * pop);
            }
            else
            {
                transform.localScale = Vector3.one * _targetBaseScale;
            }

            transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, -2.5f);

            // Fade out in synchronization with domain entity lifetime
            Color c = _baseColor;
            c.a = _baseColor.a * _entity.Alpha;
            _textMesh.color = c;
        }
    }

    /// <summary>
    /// Synchronizes DamageTextManager with Unity scene view pool.
    /// Manages an expanded pool of 64 zero-allocation views with LRU recycling.
    /// </summary>
    public class DamageTextManagerView : MonoBehaviour
    {
        private const int MaxPoolSize = 64;
        private DamageTextManager _domainManager;
        private readonly List<DamageTextView> _viewPool = new List<DamageTextView>(MaxPoolSize);

        public DamageTextManager DomainManager => _domainManager;

        public void Initialize(EventBus eventBus)
        {
            _domainManager = new DamageTextManager(eventBus, initialCapacity: 32);
            _domainManager.OnTextSpawned += SpawnTextView;
        }

        private void Update()
        {
            if (_domainManager == null) return;

            _domainManager.Update(Time.deltaTime);

            for (int i = 0; i < _viewPool.Count; i++)
            {
                var view = _viewPool[i];
                if (view != null && view.gameObject.activeSelf)
                {
                    view.UpdateView();
                }
            }
        }

        public void SpawnTextView(DamageTextEntity entity)
        {
            if (!HappyShoot.Domain.Settings.GameSettings.ShowDamageText) return;

            // 1. Find inactive view in pool
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return;
                }
            }

            // 2. Expand pool up to MaxPoolSize
            if (_viewPool.Count < MaxPoolSize)
            {
                var go = new GameObject($"DamageText_{_viewPool.Count + 1}");
                go.transform.SetParent(transform);

                var view = go.AddComponent<DamageTextView>();
                view.Bind(entity);
                _viewPool.Add(view);
                return;
            }

            // 3. Fallback: Recycle the oldest active view (index 0) so no hit numbers are lost
            if (_viewPool.Count > 0)
            {
                var recycled = _viewPool[0];
                _viewPool.RemoveAt(0);
                recycled.Bind(entity);
                _viewPool.Add(recycled);
            }
        }
    }
}

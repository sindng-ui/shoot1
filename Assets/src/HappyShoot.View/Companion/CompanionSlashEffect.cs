using UnityEngine;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Companion
{
    /// <summary>
    /// Handles visual slash arc effect and sword swinging rotation for the Warrior companion.
    /// Keeps CompanionView modular and well under 500 lines.
    /// </summary>
    public class CompanionSlashEffect : MonoBehaviour
    {
        private GameObject _slashPivotGo;
        private SpriteRenderer _slashVisualSr;
        private SpriteRenderer _weaponSr;
        private GameObject _weaponPivotGo;
        private SpriteRenderer _bodySr;

        private float _slashVisualTimer;
        private const float SlashDuration = 0.18f;
        private float _slashBaseAngle;

        public bool IsSlashing => _slashVisualTimer > 0f;

        public void Initialize(GameObject weaponPivotGo, SpriteRenderer weaponSr, SpriteRenderer bodySr)
        {
            _weaponPivotGo = weaponPivotGo;
            _weaponSr = weaponSr;
            _bodySr = bodySr;

            _slashPivotGo = new GameObject("SlashPivot");
            _slashPivotGo.transform.SetParent(transform, false);
            _slashPivotGo.transform.localPosition = Vector3.zero;

            var slashSpriteGo = new GameObject("SlashArc");
            slashSpriteGo.transform.SetParent(_slashPivotGo.transform, false);
            slashSpriteGo.transform.localPosition = new Vector3(1.35f, 0f, 0f);
            slashSpriteGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            slashSpriteGo.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);

            _slashVisualSr = slashSpriteGo.AddComponent<SpriteRenderer>();
            _slashVisualSr.sprite = WarriorSkillSpriteHelper.GetOrCreateSlashArcSprite(128);
            _slashVisualSr.color = new Color(1.0f, 0.95f, 0.4f, 0f);
            _slashVisualSr.sortingOrder = 14;
            _slashPivotGo.SetActive(false);
        }

        public void TriggerSlash(float baseAngle)
        {
            _slashBaseAngle = baseAngle;
            _slashVisualTimer = SlashDuration;
            if (_weaponSr != null) _weaponSr.sortingOrder = 14;
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - 60f;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_slashVisualSr != null) _slashVisualSr.color = Color.white;
            }
        }

        public void UpdateSlash(float dt)
        {
            if (_slashVisualTimer <= 0f) return;
            _slashVisualTimer -= dt;
            float p = Mathf.Clamp01(1.0f - (_slashVisualTimer / SlashDuration));
            float currentAngle = _slashBaseAngle + Mathf.Lerp(-60f, 60f, Mathf.SmoothStep(0f, 1f, p));

            if (_slashPivotGo != null)
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (_weaponPivotGo != null)
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (_slashVisualSr != null)
            {
                Color c = _slashVisualSr.color;
                c.a = Mathf.Sin(p * Mathf.PI) * 0.95f;
                _slashVisualSr.color = c;
            }

            if (_slashVisualTimer <= 0f)
            {
                if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
                if (_weaponSr != null) _weaponSr.sortingOrder = 13;
                if (_weaponPivotGo != null)
                {
                    bool isFlipped = _bodySr != null && _bodySr.flipX;
                    _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, isFlipped ? 135f : -45f);
                }
            }
        }
    }
}

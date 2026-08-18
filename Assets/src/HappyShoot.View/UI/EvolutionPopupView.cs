using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Unity UI View that pops up a glorious banner whenever a weapon is synthesized / evolved.
    /// </summary>
    public class EvolutionPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private Text _evolvedTitleText;
        [SerializeField] private Text _evolvedDescriptionText;

        public void BindEventBus(EventBus eventBus)
        {
            eventBus?.Subscribe<SkillEvolvedEvent>(OnSkillEvolved);

            if (_popupRoot != null)
            {
                _popupRoot.SetActive(false);
            }
        }

        private void OnSkillEvolved(SkillEvolvedEvent evt)
        {
            if (_popupRoot != null)
            {
                _popupRoot.SetActive(true);
            }

            if (_evolvedTitleText != null)
            {
                _evolvedTitleText.text = $"⚡ WEAPON EVOLVED: {evt.EvolvedSkillName} ⚡";
            }

            if (_evolvedDescriptionText != null)
            {
                _evolvedDescriptionText.text = $"Synthesized from {evt.OldSkillId} into supreme form!";
            }

            CancelInvoke(nameof(ClosePopup));
            Invoke(nameof(ClosePopup), 2.5f);
        }

        private void ClosePopup()
        {
            if (_popupRoot != null)
            {
                _popupRoot.SetActive(false);
            }
        }
    }
}

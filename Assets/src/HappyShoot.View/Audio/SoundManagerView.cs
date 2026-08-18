using UnityEngine;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.Audio
{
    /// <summary>
    /// Unity Sound Manager that listens to domain events and plays appropriate sound effects.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundManagerView : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip _attackClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private AudioClip _gemClip;
        [SerializeField] private AudioClip _levelUpClip;
        [SerializeField] private AudioClip _evolveClip;

        private AudioSource _audioSource;
        private EventBus _eventBus;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void BindEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<PlaySoundEvent>(OnPlaySound);
            _eventBus?.Subscribe<MonsterDamagedEvent>(OnMonsterDamaged);
            _eventBus?.Subscribe<ExpGainedEvent>(OnExpGained);
            _eventBus?.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
            _eventBus?.Subscribe<SkillEvolvedEvent>(OnSkillEvolved);
        }

        private void OnPlaySound(PlaySoundEvent evt)
        {
            // Play based on sound type
        }

        private void OnMonsterDamaged(MonsterDamagedEvent evt)
        {
            if (_hitClip != null) _audioSource.PlayOneShot(_hitClip, 0.4f);
        }

        private void OnExpGained(ExpGainedEvent evt)
        {
            if (_gemClip != null) _audioSource.PlayOneShot(_gemClip, 0.3f);
        }

        private void OnLevelUp(PlayerLevelUpEvent evt)
        {
            if (_levelUpClip != null) _audioSource.PlayOneShot(_levelUpClip, 0.8f);
        }

        private void OnSkillEvolved(SkillEvolvedEvent evt)
        {
            if (_evolveClip != null) _audioSource.PlayOneShot(_evolveClip, 1.0f);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Spatial;
using HappyShoot.Domain.Waves;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;

namespace HappyShoot.View.Timeline
{
    /// <summary>
    /// Unity View that displays the 15-minute survival timer (MM:SS) and controls the domain timeline.
    /// </summary>
    public class WaveTimelineView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private MonsterSpawnerView _spawnerView;
        [SerializeField] private Text _timerText;
        [SerializeField] private GameObject _bossWarningBanner;

        private WaveTimelineManager _domainTimeline;

        public WaveTimelineManager DomainTimeline => _domainTimeline;

        private void Awake()
        {
            _domainTimeline = new WaveTimelineManager();
            _domainTimeline.OnBossSpawnTriggered += OnBossSpawned;

            if (_bossWarningBanner != null)
            {
                _bossWarningBanner.SetActive(false);
            }
        }

        private void Update()
        {
            if (_domainTimeline == null || _playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            Vector2D playerPos = _playerView.Entity.Position;
            var spawner = _spawnerView != null ? _spawnerView.DomainSpawner : null;

            _domainTimeline.Update(Time.deltaTime, spawner, playerPos);

            // Update timer text MM:SS
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(_domainTimeline.ElapsedTime / 60f);
                int seconds = Mathf.FloorToInt(_domainTimeline.ElapsedTime % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void OnBossSpawned(WaveBossType bossType, Vector2D pos)
        {
            Debug.LogWarning($"[WaveTimelineView] WARNING! {bossType} has spawned!");

            if (_bossWarningBanner != null)
            {
                _bossWarningBanner.SetActive(true);
                CancelInvoke(nameof(HideWarning));
                Invoke(nameof(HideWarning), 3.0f);
            }
        }

        private void HideWarning()
        {
            if (_bossWarningBanner != null)
            {
                _bossWarningBanner.SetActive(false);
            }
        }
    }
}

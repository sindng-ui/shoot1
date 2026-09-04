using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Session;
using HappyShoot.View.Companion;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Context data bundle passed to DevCheatButtonHelper.
    /// </summary>
    public class DevCheatContext
    {
        public PlayerView PlayerView;
        public LevelSystem LevelSystem;
        public GameSessionEntity GameSession;
        public MonsterSpawnerView SpawnerView;
        public CompanionManagerView CompanionManager;
        public SkillTreeManager SkillTreeManager;
    }

    /// <summary>
    /// Modular helper that constructs the top cheat buttons row
    /// (GodMode, Heal, Level, KillAll, Speed, Gold, Gem Cheats, Phase, Companions)
    /// and handles their click logic. Keeps DevSkillSelectorUiView safely under 500 lines.
    /// </summary>
    public static class DevCheatButtonHelper
    {
        private static float _currentTimeScale = 1.0f;
        private static Text _godModeBtnText;
        private static Text _timeScaleBtnText;
        private static Text _warriorCompBtnText;
        private static Text _rangerCompBtnText;

        public static void BuildCheatSection(
            GameObject contentBox,
            ref float currentY,
            DevCheatContext ctx,
            Func<Transform, string, string, int, TextAnchor, Vector2, Vector2, Vector2, Vector2, Vector2, Color, Text> createTextFunc)
        {
            // Header Title
            createTextFunc(contentBox.transform, "Title", "🛠️ 개발자 치트 & 스킬 콘솔", 14, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, currentY), new Vector2(340f, 22f), new Color(0.3f, 1f, 0.5f));
            currentY -= 28f;

            // Row 1: GodMode & FullHeal
            CreateSmallButton(contentBox.transform, "BtnGod", "🛡️ 무적 모드: OFF", new Vector2(-85f, currentY), new Vector2(160f, 26f),
                new Color(0.3f, 0.3f, 0.35f, 1f), () => ToggleGodMode(ctx.PlayerView), out _godModeBtnText);
            CreateSmallButton(contentBox.transform, "BtnHeal", "💖 체력 풀회복", new Vector2(85f, currentY), new Vector2(160f, 26f),
                new Color(0.2f, 0.6f, 0.3f, 1f), () => HealFull(ctx.PlayerView), out _);
            currentY -= 30f;

            // Row 2: LevelUp & KillAll
            CreateSmallButton(contentBox.transform, "BtnLevelUp", "🌟 레벨 +1", new Vector2(-85f, currentY), new Vector2(160f, 26f),
                new Color(0.6f, 0.4f, 0.15f, 1f), () => ctx.LevelSystem?.AddExp(ctx.LevelSystem.RequiredExp), out _);
            CreateSmallButton(contentBox.transform, "BtnKillAll", "💀 몬스터 전멸", new Vector2(85f, currentY), new Vector2(160f, 26f),
                new Color(0.7f, 0.2f, 0.2f, 1f), () => KillAllMonsters(ctx.SpawnerView), out _);
            currentY -= 30f;

            // Row 3: Speed & Gold
            CreateSmallButton(contentBox.transform, "BtnSpeed", "⏩ 속도: 1x", new Vector2(-85f, currentY), new Vector2(160f, 26f),
                new Color(0.3f, 0.3f, 0.6f, 1f), ToggleTimeScale, out _timeScaleBtnText);
            CreateSmallButton(contentBox.transform, "BtnGold", "💰 골드 +1000", new Vector2(85f, currentY), new Vector2(160f, 26f),
                new Color(0.75f, 0.65f, 0.1f, 1f), () => ctx.GameSession?.AddGold(1000), out _);
            currentY -= 30f;

            // Row 4: 💎 Gem Cheats (+10 for each gem)
            CreateSmallButton(contentBox.transform, "BtnRuby", "🔴 루비 +10", new Vector2(-112f, currentY), new Vector2(106f, 26f),
                new Color(0.75f, 0.20f, 0.20f, 1f), () => AddGems(ctx.SkillTreeManager, GemType.Ruby, 10), out _);
            CreateSmallButton(contentBox.transform, "BtnEmerald", "🟢 에메랄드 +10", new Vector2(0f, currentY), new Vector2(106f, 26f),
                new Color(0.18f, 0.65f, 0.30f, 1f), () => AddGems(ctx.SkillTreeManager, GemType.Emerald, 10), out _);
            CreateSmallButton(contentBox.transform, "BtnAmethyst", "🟣 아메시스트 +10", new Vector2(112f, currentY), new Vector2(106f, 26f),
                new Color(0.60f, 0.25f, 0.75f, 1f), () => AddGems(ctx.SkillTreeManager, GemType.Amethyst, 10), out _);
            currentY -= 30f;

            // Row 5: 💎 All Gems +50 (Bulk cheat for forge test)
            CreateSmallButton(contentBox.transform, "BtnAllGems", "💎 보석 전체 +50 (대장간 테스트)", new Vector2(0f, currentY), new Vector2(330f, 26f),
                new Color(0.25f, 0.50f, 0.85f, 1f), () => AddAllGems(ctx.SkillTreeManager, 50), out _);
            currentY -= 30f;

            // Row 6: Phase Jump Buttons
            CreateSmallButton(contentBox.transform, "BtnP1", "1️⃣ Phase 1", new Vector2(-112f, currentY), new Vector2(106f, 26f),
                new Color(0.20f, 0.45f, 0.70f, 1f), () => ctx.SpawnerView?.JumpToPhase(1), out _);
            CreateSmallButton(contentBox.transform, "BtnP2", "2️⃣ Phase 2", new Vector2(0f, currentY), new Vector2(106f, 26f),
                new Color(0.65f, 0.35f, 0.15f, 1f), () => ctx.SpawnerView?.JumpToPhase(2), out _);
            CreateSmallButton(contentBox.transform, "BtnP3", "3️⃣ Phase 3", new Vector2(112f, currentY), new Vector2(106f, 26f),
                new Color(0.55f, 0.15f, 0.65f, 1f), () => ctx.SpawnerView?.JumpToPhase(3), out _);
            currentY -= 30f;

            // Row 7: Companion Cheat Toggle Buttons
            CreateSmallButton(contentBox.transform, "BtnWarComp", "🛡️ 전사 동료: OFF", new Vector2(-85f, currentY), new Vector2(160f, 26f),
                new Color(0.7f, 0.3f, 0.2f, 1f), () => ToggleWarriorComp(ctx.CompanionManager), out _warriorCompBtnText);
            CreateSmallButton(contentBox.transform, "BtnRngComp", "🏹 궁수 동료: OFF", new Vector2(85f, currentY), new Vector2(160f, 26f),
                new Color(0.2f, 0.6f, 0.4f, 1f), () => ToggleRangerComp(ctx.CompanionManager), out _rangerCompBtnText);
            currentY -= 30f;

            // Row 8: Dimension Portal & Side-Scroll Instant Trigger
            CreateSmallButton(contentBox.transform, "BtnPortal", "🌀 차원 포탈 소환", new Vector2(-85f, currentY), new Vector2(160f, 26f),
                new Color(0.5f, 0.2f, 0.8f, 1f), () =>
                {
                    Vector3 spawnPos = ctx.PlayerView != null ? ctx.PlayerView.transform.position + new Vector3(2.5f, 0f, 0f) : Vector3.zero;
                    ctx.SpawnerView?.SpawnDimensionPortal(spawnPos);
                }, out _);
            CreateSmallButton(contentBox.transform, "BtnSideScroll", "🚀 횡스크롤 즉시 진입", new Vector2(85f, currentY), new Vector2(160f, 26f),
                new Color(0.2f, 0.7f, 0.85f, 1f), () => SideScroll.SideScrollModeController.Instance?.EnterSideScrollMode(), out _);
            currentY -= 32f;
        }

        public static void UpdateCompanionButtons(CompanionManagerView companionManager)
        {
            if (_warriorCompBtnText != null && companionManager != null)
                _warriorCompBtnText.text = companionManager.IsWarriorActive ? "🛡️ 전사 동료: ON" : "🛡️ 전사 동료: OFF";
            if (_rangerCompBtnText != null && companionManager != null)
                _rangerCompBtnText.text = companionManager.IsRangerActive ? "🏹 궁수 동료: ON" : "🏹 궁수 동료: OFF";
        }

        private static void AddGems(SkillTreeManager manager, GemType type, int amount)
        {
            if (manager == null) return;
            manager.SaveData.AddGems(type, amount);
            manager.Save();
            Debug.Log($"[DevCheat] Added +{amount} {type}! Total: {manager.SaveData.GetGems(type)}");
        }

        private static void AddAllGems(SkillTreeManager manager, int amount)
        {
            if (manager == null) return;
            manager.SaveData.AddGems(GemType.Ruby, amount);
            manager.SaveData.AddGems(GemType.Emerald, amount);
            manager.SaveData.AddGems(GemType.Amethyst, amount);
            manager.Save();
            Debug.Log($"[DevCheat] Added +{amount} to All Gems! Ruby={manager.SaveData.GetGems(GemType.Ruby)}, Emerald={manager.SaveData.GetGems(GemType.Emerald)}, Amethyst={manager.SaveData.GetGems(GemType.Amethyst)}");
        }

        private static void ToggleGodMode(PlayerView playerView)
        {
            if (playerView?.Entity == null) return;
            var p = playerView.Entity;
            p.IsGodMode = !p.IsGodMode;
            if (_godModeBtnText != null)
                _godModeBtnText.text = p.IsGodMode ? "🛡️ 무적 모드: ON" : "🛡️ 무적 모드: OFF";
        }

        private static void HealFull(PlayerView playerView)
        {
            if (playerView?.Entity == null) return;
            playerView.Entity.Heal(999999f);
        }

        private static void KillAllMonsters(MonsterSpawnerView spawnerView)
        {
            if (spawnerView == null) return;
            var activeList = spawnerView.DomainSpawner?.ActiveMonsters;
            if (activeList == null) return;
            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                if (activeList[i].IsActive && !activeList[i].IsDead) activeList[i].TakeDamage(999999f);
            }
        }

        private static void ToggleTimeScale()
        {
            if (Mathf.Approximately(_currentTimeScale, 1.0f)) _currentTimeScale = 2.0f;
            else if (Mathf.Approximately(_currentTimeScale, 2.0f)) _currentTimeScale = 4.0f;
            else if (Mathf.Approximately(_currentTimeScale, 4.0f)) _currentTimeScale = 0.5f;
            else _currentTimeScale = 1.0f;

            Time.timeScale = _currentTimeScale;
            if (_timeScaleBtnText != null) _timeScaleBtnText.text = $"⏩ 속도: {_currentTimeScale}x";
        }

        private static void ToggleWarriorComp(CompanionManagerView companionManager)
        {
            companionManager?.ToggleWarrior();
            UpdateCompanionButtons(companionManager);
        }

        private static void ToggleRangerComp(CompanionManagerView companionManager)
        {
            companionManager?.ToggleRanger();
            UpdateCompanionButtons(companionManager);
        }

        private static void CreateSmallButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, Color color, Action onClick, out Text textComp)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.color = color;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            textComp = txtGo.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Utils.FontHelper.GetKoreanFont();
            textComp.fontSize = 11;
            textComp.fontStyle = FontStyle.Bold;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
        }
    }
}

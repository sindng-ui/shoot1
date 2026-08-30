using System;
using UnityEngine;
using HappyShoot.Domain.Skills;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Dedicated configurator helper for Companion AI & Balance tuning sliders in the Combat Sandbox.
    /// Strictly modular and under 500 lines to keep SkillTuningRowConfigurator well under limits.
    /// </summary>
    public static class SkillTuningCompanionConfigurator
    {
        public static void ConfigureCompanionRows(
            SkillConfigData config,
            Transform container,
            Action<GameObject> onRowCreated)
        {
            if (container == null || config == null) return;
            if (config.Companion == null) config.Companion = new CompanionTuningConfig();

            var comp = config.Companion;
            float yOffset = 0f;

            void AddRow(string title, float curVal, float min, float max, float step, Action<float> onChanged, bool isInt = false)
            {
                var row = SkillTuningSliderFactory.CreateSliderRow(container, title, curVal, min, max, step, onChanged, ref yOffset, isInt);
                onRowCreated?.Invoke(row);
            }

            // 1. 공격력 및 패시브 배율
            AddRow("⚔️ 최종 공격력 보정 (%)", comp.FinalDamageScale * 100f, 0f, 100f, 1f, v => comp.FinalDamageScale = v * 0.01f, isInt: true);
            AddRow("🧬 패시브 효과 보정 (%)", comp.PassiveScale * 100f, 0f, 100f, 1f, v => comp.PassiveScale = v * 0.01f, isInt: true);

            // 2. 행동 반경 및 이동
            AddRow("⭕ 주인공 주변 반경 (m)", comp.RegroupRadius, 1.0f, 10.0f, 0.5f, v => comp.RegroupRadius = v);
            AddRow("🛡️ 마법사 호위 안착거리 (m)", comp.RegroupArrivalDistance, 1.0f, 5.0f, 0.2f, v => comp.RegroupArrivalDistance = v);
            AddRow("🏃 이동속도 배율 (%)", comp.MoveSpeedMultiplier * 100f, 50f, 200f, 5f, v => comp.MoveSpeedMultiplier = v * 0.01f, isInt: true);

            // 3. 교전 사거리
            AddRow("⚔️ 전사 추적 교전 사거리 (m)", comp.WarriorEngageRange, 1.0f, 6.0f, 0.2f, v => comp.WarriorEngageRange = v);
            AddRow("🏹 궁수 원거리 저격 사거리 (m)", comp.RangerSnipingRange, 5.0f, 18.0f, 0.5f, v => comp.RangerSnipingRange = v);

            // 4. 타겟팅 우선순위 토글 (0 = 자신 근접 / 1 = 마법사 경호)
            AddRow("🎯 타겟 우선순위 (0:근접 / 1:경호)", comp.PrioritizeProtectWizard ? 1f : 0f, 0f, 1f, 1f, v => comp.PrioritizeProtectWizard = (v >= 0.5f), isInt: true);
        }
    }
}

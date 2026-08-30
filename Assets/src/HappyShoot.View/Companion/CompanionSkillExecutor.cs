using System;
using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Config;
using HappyShoot.View.Monsters;
using HappyShoot.View.Projectiles;

namespace HappyShoot.View.Companion
{
    /// <summary>
    /// Dedicated helper for executing companion active skills with full sandbox tuning and 1/3 damage scaling.
    /// Strictly modular and under 500 lines to preserve CompanionView cleanly within limits.
    /// </summary>
    public static class CompanionSkillExecutor
    {
        public static void ExecuteSkill(
            string skillId,
            int skillLevel,
            CompanionEntity entity,
            Vector2 attackDir,
            MonsterSpawnerView spawnerView,
            ProjectileManagerView projManager,
            EventBus eventBus,
            Action<float> onSlashTriggered,
            Transform companionTransform = null)
        {
            if (entity == null) return;
            var cfg = SkillConfigRepository.Instance.GetConfig();
            float areaMult = entity.GetEffectiveAreaMultiplier();

            switch (skillId)
            {
                // ================= WARRIOR SKILLS =================
                case "slash":
                    ExecuteSlash(cfg.Slash.Damage, cfg.Slash.Radius * areaMult, cfg.Slash.ArcAngle, entity, attackDir, spawnerView, eventBus, onSlashTriggered);
                    break;

                case "ground_stomp":
                    ExecuteGroundStomp(cfg.GroundStomp.Damage, cfg.GroundStomp.Length * areaMult, cfg.GroundStomp.Radius, skillLevel, entity, attackDir, spawnerView, eventBus);
                    break;

                case "whirlwind":
                    ExecuteWhirlwind(cfg.Whirlwind.Damage, cfg.Whirlwind.Radius * areaMult, entity, spawnerView, eventBus);
                    break;

                // ================= RANGER SKILLS =================
                case "bow":
                    ExecuteBow(cfg.Bow.Damage, cfg.Bow.Speed, cfg.Bow.ArrowCount, cfg.Bow.SpreadAngle, skillLevel, entity, attackDir, projManager, eventBus);
                    break;

                case "glaive":
                    ExecuteGlaive(cfg.Glaive.Damage, cfg.Glaive.Speed, cfg.Glaive.Distance, entity, attackDir, companionTransform, eventBus);
                    break;

                case "arrow_rain":
                    ExecuteArrowRain(cfg.ArrowRain.Damage, cfg.ArrowRain.Radius * areaMult, entity, attackDir, spawnerView, eventBus);
                    break;

                default:
                    // Fallback to default class basic attack
                    if (entity.Type == CompanionType.Warrior)
                        ExecuteSlash(35f, 2.5f * areaMult, 150f, entity, attackDir, spawnerView, eventBus, onSlashTriggered);
                    else
                        ExecuteBow(25f, 16f, 1, 0f, 1, entity, attackDir, projManager, eventBus);
                    break;
            }
        }

        private static void ExecuteSlash(
            float baseDmg, float radius, float arcAngle,
            CompanionEntity entity, Vector2 attackDir,
            MonsterSpawnerView spawnerView, EventBus eventBus,
            Action<float> onSlashTriggered)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            float baseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

            onSlashTriggered?.Invoke(baseAngle);

            var activeMonsters = spawnerView?.DomainSpawner?.ActiveMonsters;
            if (activeMonsters != null)
            {
                Vector2D myPos = entity.Position;
                float dotThreshold = Mathf.Cos(arcAngle * 0.5f * Mathf.Deg2Rad);

                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m == null || m.IsDead) continue;
                    if ((m.Position - myPos).SqrMagnitude <= radius * radius)
                    {
                        Vector2 toM = new Vector2((float)(m.Position.X - myPos.X), (float)(m.Position.Y - myPos.Y)).normalized;
                        if (Vector2.Dot(attackDir, toM) >= dotThreshold)
                            m.TakeDamage(actualDamage, isCritical: false);
                    }
                }
            }
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack));
        }

        private static void ExecuteGroundStomp(
            float baseDmg, float length, float width, int level,
            CompanionEntity entity, Vector2 attackDir,
            MonsterSpawnerView spawnerView, EventBus eventBus)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            var activeMonsters = spawnerView?.DomainSpawner?.ActiveMonsters;
            if (activeMonsters != null)
            {
                Vector2D myPos = entity.Position;
                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m == null || m.IsDead) continue;

                    Vector2 toM = new Vector2((float)(m.Position.X - myPos.X), (float)(m.Position.Y - myPos.Y));
                    float projDist = Vector2.Dot(toM, attackDir);
                    if (projDist >= 0f && projDist <= length)
                    {
                        Vector2 perp = toM - (attackDir * projDist);
                        if (perp.sqrMagnitude <= width * width)
                        {
                            m.TakeDamage(actualDamage, isCritical: false);
                        }
                    }
                }
            }

            int steps = 5;
            var stepPositions = new Vector2D[steps];
            for (int s = 0; s < steps; s++)
            {
                float dist = (length / steps) * (s + 1);
                stepPositions[s] = entity.Position + new Vector2D(attackDir.x * dist, attackDir.y * dist);
            }
            eventBus?.Publish(new GroundStompExecutedEvent(entity.Position, new Vector2D(attackDir.x, attackDir.y), length, width, level, stepPositions));
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack));
        }

        private static void ExecuteWhirlwind(
            float baseDmg, float radius,
            CompanionEntity entity, MonsterSpawnerView spawnerView, EventBus eventBus)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            var activeMonsters = spawnerView?.DomainSpawner?.ActiveMonsters;
            if (activeMonsters != null)
            {
                Vector2D myPos = entity.Position;
                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m == null || m.IsDead) continue;
                    if ((m.Position - myPos).SqrMagnitude <= radius * radius)
                    {
                        m.TakeDamage(actualDamage, isCritical: false);
                    }
                }
            }

            eventBus?.Publish(new WhirlwindExecutedEvent(entity.Position, radius));
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack));
        }

        private static void ExecuteBow(
            float baseDmg, float speed, int count, float spreadAngle, int level,
            CompanionEntity entity, Vector2 attackDir,
            ProjectileManagerView projManager, EventBus eventBus)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            int totalArrows = count + (level - 1);

            if (projManager?.DomainManager != null)
            {
                if (totalArrows <= 1)
                {
                    projManager.DomainManager.LaunchProjectile(entity.Position, new Vector2D(attackDir.x, attackDir.y), speed, actualDamage, 999);
                }
                else
                {
                    float baseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
                    float halfSpread = spreadAngle * 0.5f;
                    for (int i = 0; i < totalArrows; i++)
                    {
                        float t = (totalArrows == 1) ? 0.5f : (float)i / (totalArrows - 1);
                        float angle = (baseAngle - halfSpread) + (spreadAngle * t);
                        float rad = angle * Mathf.Deg2Rad;
                        Vector2D dir = new Vector2D(Mathf.Cos(rad), Mathf.Sin(rad));
                        projManager.DomainManager.LaunchProjectile(entity.Position, dir, speed, actualDamage, 999);
                    }
                }
            }
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot));
        }

        private static void ExecuteGlaive(
            float baseDmg, float speed, float distance,
            CompanionEntity entity, Vector2 attackDir,
            Transform companionTransform, EventBus eventBus)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            Vector2 origin = companionTransform != null
                ? (Vector2)companionTransform.position
                : new Vector2((float)entity.Position.X, (float)entity.Position.Y);

            // Use WindGlaiveManagerView to return exactly to the companion archer!
            if (WindGlaiveManagerView.Instance != null)
            {
                WindGlaiveManagerView.Instance.LaunchGlaive(origin, attackDir, actualDamage, distance, speed, count: 1, returnTarget: companionTransform);
            }
            else
            {
                eventBus?.Publish(new WindGlaiveExecutedEvent(new Vector2D(origin.x, origin.y), new Vector2D(attackDir.x, attackDir.y), actualDamage, distance, speed, glaiveCount: 1));
            }
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot));
        }

        private static void ExecuteArrowRain(
            float baseDmg, float radius,
            CompanionEntity entity, Vector2 attackDir,
            MonsterSpawnerView spawnerView, EventBus eventBus)
        {
            float actualDamage = entity.CalculateFinalDamage(baseDmg);
            Vector2 rainCenter = new Vector2((float)entity.Position.X, (float)entity.Position.Y) + attackDir * 3.5f;
            Vector2D rainCenter2D = new Vector2D(rainCenter.x, rainCenter.y);

            // Publish ArrowRainExecutedEvent to render barrage of arrows falling from sky!
            eventBus?.Publish(new ArrowRainExecutedEvent(rainCenter2D, radius, duration: 1.5f, arrowCount: 20, damagePerArrow: actualDamage));
            eventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot));
        }
    }
}

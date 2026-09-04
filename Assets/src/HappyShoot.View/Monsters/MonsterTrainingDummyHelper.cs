using System;
using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Helper class for spawning training dummy golems and bats in test/sandbox modes.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class MonsterTrainingDummyHelper
    {
        public static void SpawnTrainingDummies(
            Vector2D center,
            int count,
            MonsterSpawner domainSpawner,
            Dictionary<int, MonsterView> activeViewMap,
            List<MonsterView> viewPool,
            Func<MonsterEntity, MonsterView> getOrCreateView)
        {
            if (domainSpawner == null) return;

            domainSpawner.DespawnAll();
            activeViewMap?.Clear();
            if (viewPool != null)
            {
                for (int v = 0; v < viewPool.Count; v++)
                {
                    if (viewPool[v] != null) viewPool[v].gameObject.SetActive(false);
                }
            }

            float radius = 3.0f;
            float step = (Mathf.PI * 2f) / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                float angle = i * step;
                Vector2D pos = center + new Vector2D(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);
                var monster = domainSpawner.SpawnMonster("훈련용 허수아비", 999999f, 0f, 0f, 1, 0, pos, MonsterType.Golem);
                getOrCreateView?.Invoke(monster);
            }

            SpawnBatDummies(center, 12, domainSpawner, getOrCreateView);
        }

        public static void SpawnBatDummies(
            Vector2D center,
            int batCount,
            MonsterSpawner domainSpawner,
            Func<MonsterEntity, MonsterView> getOrCreateView)
        {
            if (domainSpawner == null) return;

            for (int i = 0; i < batCount; i++)
            {
                float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float dist = UnityEngine.Random.Range(3.5f, 6.5f);
                Vector2D pos = center + new Vector2D(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                var bat = domainSpawner.SpawnMonster("박쥐 허수아비", 999999f, 0f, 0f, 1, 0, pos, MonsterType.Bat);
                getOrCreateView?.Invoke(bat);
            }
        }
    }
}

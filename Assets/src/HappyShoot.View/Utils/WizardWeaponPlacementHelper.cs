using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Pure calculation helper for wizard staff placement, angles, and hand-snapping.
    /// Strictly modular and easily unit-testable across all 8+ view directions.
    /// </summary>
    public static class WizardWeaponPlacementHelper
    {
        public struct StaffPlacement
        {
            public Vector3 LocalPosition;
            public float RotationZ;
            public bool FlipX;
            public int SortingOrder;
            public Vector3 Scale;
            public Vector2 HandCenterWorld; // The actual target right-hand position for verification
        }

        /// <summary>
        /// Calculates exact staff placement so it is gripped firmly in the wizard's dominant right hand.
        /// </summary>
        public static StaffPlacement CalculatePlacement(
            HeroSpriteHelper.ViewDirection viewDir,
            bool isFacingLeft,
            float castPulseProgress = 0f)
        {
            float pulse = Mathf.Sin(castPulseProgress * Mathf.PI);
            float castAngleOffset = isFacingLeft ? -pulse * 14f : pulse * 14f;
            float castHeightOffset = pulse * 0.05f;

            bool isDiag = (viewDir == HeroSpriteHelper.ViewDirection.FrontDiagonal);
            bool isSide = (viewDir == HeroSpriteHelper.ViewDirection.Side);
            bool isBack = (viewDir == HeroSpriteHelper.ViewDirection.Back || viewDir == HeroSpriteHelper.ViewDirection.BackDiagonal);

            // Dominant right-hand position:
            // - FrontDiagonal: Screen-right hand (+0.19m) when facing left (SW), Screen-left hand (-0.19m) when facing right (SE)
            // - Side profile: Hand is +0.08m (East) or -0.08m (West)
            // - Front / Back: Symmetrical hand offsets
            float handX = isSide ? (isFacingLeft ? -0.08f : 0.08f) : (isFacingLeft ? 0.19f : -0.19f);
            float handY = isSide ? -0.10f : -0.09f;

            float baseAngle = isSide ? (isFacingLeft ? 25f : -25f) : (isFacingLeft ? -25f : 25f);
            bool weaponFlip = isSide ? isFacingLeft : !isFacingLeft;
            int sortingOrder = isBack ? 14 : 16;

            return new StaffPlacement
            {
                LocalPosition = new Vector3(handX, handY + castHeightOffset, 0f),
                RotationZ = baseAngle + castAngleOffset,
                FlipX = weaponFlip,
                SortingOrder = sortingOrder,
                Scale = Vector3.one * 1.2f,
                HandCenterWorld = new Vector2(handX, handY)
            };
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Tests
{
    [TestFixture]
    public class WizardStaffPlacementTests
    {
        private const float MaxAllowedHandTolerance = 0.015f;

        [Test]
        [TestCase(HeroSpriteHelper.ViewDirection.FrontDiagonal, false, -0.19f, -0.09f, 25f, true, Description = "SE (SouthEast / 오른쪽 아래): 오른손(화면 좌측)에 정확히 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.FrontDiagonal, true, 0.19f, -0.09f, -25f, false, Description = "SW (SouthWest / 왼쪽 아래): flipX된 오른손(화면 우측)에 정확히 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.Side, false, 0.08f, -0.10f, -25f, false, Description = "East (동쪽 / 오른쪽): 측면 앞손에 정확히 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.Side, true, -0.08f, -0.10f, 25f, true, Description = "West (서쪽 / 왼쪽): 측면 앞손에 정확히 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.Front, false, -0.19f, -0.09f, 25f, true, Description = "South (정면): 오른손 위치에 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.Front, true, 0.19f, -0.09f, -25f, false, Description = "South (정면 좌측 반전): 대칭 오른손에 스냅")]
        [TestCase(HeroSpriteHelper.ViewDirection.Back, false, -0.19f, -0.09f, 25f, true, Description = "North (후면): 등 뒤 레이어링 적용")]
        [TestCase(HeroSpriteHelper.ViewDirection.BackDiagonal, false, -0.19f, -0.09f, 25f, true, Description = "NE (후면 대각선 우): 등 뒤 레이어링 적용")]
        [TestCase(HeroSpriteHelper.ViewDirection.BackDiagonal, true, 0.19f, -0.09f, -25f, false, Description = "NW (후면 대각선 좌): 등 뒤 레이어링 적용")]
        public void CalculatePlacement_AllDirections_StaffSnappedDirectlyToRightHand(
            HeroSpriteHelper.ViewDirection viewDir,
            bool isFacingLeft,
            float expectedX,
            float expectedY,
            float expectedAngleZ,
            bool expectedFlipX)
        {
            // Act
            var placement = WizardWeaponPlacementHelper.CalculatePlacement(viewDir, isFacingLeft, 0f);

            // Assert 1: Staff Pivot must match the Right Hand location within tolerance
            float distToHand = Vector2.Distance(
                new Vector2(placement.LocalPosition.x, placement.LocalPosition.y),
                new Vector2(expectedX, expectedY)
            );
            Assert.LessOrEqual(distToHand, MaxAllowedHandTolerance,
                $"[Direction: {viewDir}, FacingLeft: {isFacingLeft}] Staff position ({placement.LocalPosition.x:F3}, {placement.LocalPosition.y:F3}) diverged from right hand ({expectedX:F3}, {expectedY:F3}) by {distToHand:F3}m!");

            // Assert 2: Staff Rotation Angle matches expected outward tilt
            Assert.AreEqual(expectedAngleZ, placement.RotationZ, 0.1f,
                $"[Direction: {viewDir}] Expected AngleZ {expectedAngleZ}, got {placement.RotationZ}");

            // Assert 3: Staff FlipX matches the outward pointing direction
            Assert.AreEqual(expectedFlipX, placement.FlipX,
                $"[Direction: {viewDir}] Expected FlipX {expectedFlipX}, got {placement.FlipX}");

            // Assert 4: Staff Scale must be balanced (1.2x chibi ratio)
            Assert.AreEqual(1.2f, placement.Scale.x, 0.01f);
        }

        [Test]
        [TestCase(HeroSpriteHelper.ViewDirection.Back, true, 15, Description = "후면에서는 지팡이가 캐릭터 몸통 뒤에 렌더링")]
        [TestCase(HeroSpriteHelper.ViewDirection.BackDiagonal, false, 15, Description = "후면 대각선에서는 지팡이가 캐릭터 뒤에 렌더링")]
        [TestCase(HeroSpriteHelper.ViewDirection.Front, false, 17, Description = "정면에서는 지팡이가 캐릭터 손 앞에 렌더링")]
        [TestCase(HeroSpriteHelper.ViewDirection.FrontDiagonal, false, 17, Description = "앞 대각선에서는 지팡이가 손 앞에 렌더링")]
        [TestCase(HeroSpriteHelper.ViewDirection.Side, false, 17, Description = "측면에서는 지팡이가 손 앞에 렌더링")]
        public void CalculatePlacement_SortingOrder_RendersCorrectLayer(
            HeroSpriteHelper.ViewDirection viewDir,
            bool isFacingLeft,
            int expectedSortingOrder)
        {
            var placement = WizardWeaponPlacementHelper.CalculatePlacement(viewDir, isFacingLeft, 0f);
            Assert.AreEqual(expectedSortingOrder, placement.SortingOrder);
        }

        [Test]
        public void CalculatePlacement_CastingPulse_SmoothlyAnimatesElevationWithoutDetachingHand()
        {
            // During casting pulse, staff should bob up slightly but stay firmly rooted near the hand
            var basePlacement = WizardWeaponPlacementHelper.CalculatePlacement(HeroSpriteHelper.ViewDirection.FrontDiagonal, false, 0f);
            var midPulsePlacement = WizardWeaponPlacementHelper.CalculatePlacement(HeroSpriteHelper.ViewDirection.FrontDiagonal, false, 0.5f);

            Assert.Greater(midPulsePlacement.LocalPosition.y, basePlacement.LocalPosition.y,
                "Staff should rise slightly during casting pulse peak.");
            Assert.Less(Mathf.Abs(midPulsePlacement.LocalPosition.x - basePlacement.LocalPosition.x), 0.001f,
                "Horizontal hand alignment must not drift during pulse.");
        }
    }
}

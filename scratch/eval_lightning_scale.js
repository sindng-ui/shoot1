// Verify scale calculations for chain lightning
const spriteW = 10.24; // 1024px / 100
const spriteH = 3.12;  // 312px / 100

function testScale(dist) {
    const scaleX = dist / spriteW;
    // Natural aspect ratio scale with clamped range
    const scaleY = Math.min(0.75, Math.max(0.22, scaleX * 1.15));
    const worldWidth = scaleX * spriteW;
    const worldHeight = scaleY * spriteH;
    const originalAspect = spriteW / spriteH; // 3.28
    const currentAspect = worldWidth / worldHeight;
    console.log(`Dist: ${dist.toFixed(1)}m => scaleX: ${scaleX.toFixed(3)}, scaleY: ${scaleY.toFixed(3)} | World: ${worldWidth.toFixed(2)}m x ${worldHeight.toFixed(2)}m | Aspect: ${currentAspect.toFixed(2)} : 1 (Original: ${originalAspect.toFixed(2)})`);
}

console.log("=== Lightning Scale Evaluation ===");
testScale(1.5);
testScale(3.0);
testScale(5.0);
testScale(7.5);

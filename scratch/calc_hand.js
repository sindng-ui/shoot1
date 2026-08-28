const fs = require('fs');
const { PNG } = require('pngjs');

const p = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';
const data = PNG.sync.read(fs.readFileSync(p));
const w = data.width, h = data.height;

// Find screen-left hand (X < 175) and screen-right hand (X > 175)
let leftHand = { sumX: 0, sumY: 0, count: 0 };
let rightHand = { sumX: 0, sumY: 0, count: 0 };

for (let y = 300; y < 390; y++) {
  for (let x = 50; x < 320; x++) {
    const idx = (y * w + x) << 2;
    const a = data.data[idx + 3];
    const r = data.data[idx], g = data.data[idx+1], b = data.data[idx+2];
    if (a > 150) {
      // Glove dark purple/grey:
      if (r >= 30 && r <= 70 && g >= 25 && g <= 65 && b >= 45 && b <= 95 && Math.abs(r-g) < 20) {
        if (x < 175) {
          leftHand.sumX += x; leftHand.sumY += y; leftHand.count++;
        } else {
          rightHand.sumX += x; rightHand.sumY += y; rightHand.count++;
        }
      }
    }
  }
}

const leftAvgX = leftHand.sumX / leftHand.count;
const leftAvgY = leftHand.sumY / leftHand.count;
const rightAvgX = rightHand.sumX / rightHand.count;
const rightAvgY = rightHand.sumY / rightHand.count;

console.log(`Screen-Left hand center: (${leftAvgX.toFixed(1)}, ${leftAvgY.toFixed(1)}) [count=${leftHand.count}]`);
console.log(`Screen-Right hand center: (${rightAvgX.toFixed(1)}, ${rightAvgY.toFixed(1)}) [count=${rightHand.count}]`);

// Convert to local world coordinates in PlayerView:
// Center is (175, 135 from bottom = 450 - 135 = 315)
// PPU = 450, Scale = 1.5
function toWorld(px, py) {
  const wx = (px - 175) / 450 * 1.5;
  const wy = (315 - py) / 450 * 1.5;
  return { wx: wx.toFixed(3), wy: wy.toFixed(3) };
}

console.log(`Screen-Left hand world offset:`, toWorld(leftAvgX, leftAvgY));
console.log(`Screen-Right hand world offset:`, toWorld(rightAvgX, rightAvgY));

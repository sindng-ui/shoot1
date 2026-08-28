const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

function pixelate(srcPng, pixelSize, colorStep = 8) {
  const w = srcPng.width;
  const h = srcPng.height;
  const outPng = new PNG({ width: w, height: h });

  for (let i = 0; i < w * h * 4; i++) outPng.data[i] = 0;

  for (let by = 0; by < h; by += pixelSize) {
    for (let bx = 0; bx < w; bx += pixelSize) {
      let rSum = 0, gSum = 0, bSum = 0, weightSum = 0;
      let opaqueSamples = 0;
      let totalSamples = 0;

      for (let dy = 0; dy < pixelSize && (by + dy) < h; dy++) {
        for (let dx = 0; dx < pixelSize && (bx + dx) < w; dx++) {
          const idx = ((by + dy) * w + (bx + dx)) << 2;
          const a = srcPng.data[idx + 3];
          if (a > 30) {
            const weight = a / 255.0;
            rSum += srcPng.data[idx] * weight;
            gSum += srcPng.data[idx + 1] * weight;
            bSum += srcPng.data[idx + 2] * weight;
            weightSum += weight;
            opaqueSamples++;
          }
          totalSamples++;
        }
      }

      if (opaqueSamples >= Math.max(1, Math.floor(totalSamples * 0.30)) && weightSum > 0) {
        let avgR = Math.round(rSum / weightSum);
        let avgG = Math.round(gSum / weightSum);
        let avgB = Math.round(bSum / weightSum);

        if (colorStep > 1) {
          avgR = Math.round(avgR / colorStep) * colorStep;
          avgG = Math.round(avgG / colorStep) * colorStep;
          avgB = Math.round(avgB / colorStep) * colorStep;
          avgR = Math.min(255, Math.max(0, avgR));
          avgG = Math.min(255, Math.max(0, avgG));
          avgB = Math.min(255, Math.max(0, avgB));
        }

        for (let dy = 0; dy < pixelSize && (by + dy) < h; dy++) {
          for (let dx = 0; dx < pixelSize && (bx + dx) < w; dx++) {
            const outIdx = ((by + dy) * w + (bx + dx)) << 2;
            outPng.data[outIdx]     = avgR;
            outPng.data[outIdx + 1] = avgG;
            outPng.data[outIdx + 2] = avgB;
            outPng.data[outIdx + 3] = 255;
          }
        }
      }
    }
  }

  return outPng;
}

const warriorPath = '/mnt/k/unityprojects/shoot1/shoot1/scratch/original_smooth_sprites/Warrior/warrior_front.png';
const rangerPath = '/mnt/k/unityprojects/shoot1/shoot1/scratch/original_smooth_sprites/Ranger/ranger_front.png';
const wizardPath = '/mnt/k/unityprojects/shoot1/shoot1/scratch/original_smooth_sprites/Wizard/wizard_front.png';

for (const bs of [6]) {
  const wPng = pixelate(PNG.sync.read(fs.readFileSync(warriorPath)), bs, 8);
  fs.writeFileSync(`/mnt/k/unityprojects/shoot1/shoot1/scratch/test_warrior_bs${bs}.png`, PNG.sync.write(wPng));
  
  const rPng = pixelate(PNG.sync.read(fs.readFileSync(rangerPath)), bs, 8);
  fs.writeFileSync(`/mnt/k/unityprojects/shoot1/shoot1/scratch/test_ranger_bs${bs}.png`, PNG.sync.write(rPng));
  
  const wizPng = pixelate(PNG.sync.read(fs.readFileSync(wizardPath)), bs, 8);
  fs.writeFileSync(`/mnt/k/unityprojects/shoot1/shoot1/scratch/test_wizard_bs${bs}.png`, PNG.sync.write(wizPng));
}

console.log('Done generating bs6 samples for all 3 heroes!');

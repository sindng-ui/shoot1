const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

function pixelate(srcPng, pixelSize, colorStep = 8) {
  const w = srcPng.width;
  const h = srcPng.height;
  const outPng = new PNG({ width: w, height: h });

  // Clear
  for (let i = 0; i < w * h * 4; i++) outPng.data[i] = 0;

  for (let by = 0; by < h; by += pixelSize) {
    for (let bx = 0; bx < w; bx += pixelSize) {
      let rSum = 0, gSum = 0, bSum = 0, aSum = 0;
      let count = 0;
      let opaqueCount = 0;

      for (let dy = 0; dy < pixelSize && (by + dy) < h; dy++) {
        for (let dx = 0; dx < pixelSize && (bx + dx) < w; dx++) {
          const idx = ((by + dy) * w + (bx + dx)) << 2;
          const a = srcPng.data[idx + 3];
          if (a > 30) {
            rSum += srcPng.data[idx];
            gSum += srcPng.data[idx + 1];
            bSum += srcPng.data[idx + 2];
            opaqueCount++;
          }
          aSum += a;
          count++;
        }
      }

      // If more than 35% of the block is opaque, make this pixel block solid
      if (opaqueCount >= count * 0.35) {
        let avgR = Math.round(rSum / opaqueCount);
        let avgG = Math.round(gSum / opaqueCount);
        let avgB = Math.round(bSum / opaqueCount);

        // Optional slight color quantization for crisp retro feel
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

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior_front.png';
const srcPng = PNG.sync.read(fs.readFileSync(inputPath));

for (const blockSize of [3, 4, 5]) {
  const pix = pixelate(srcPng, blockSize, 6);
  const outPath = `/mnt/k/unityprojects/shoot1/shoot1/scratch/test_warrior_block${blockSize}.png`;
  fs.writeFileSync(outPath, PNG.sync.write(pix));
  console.log(`Saved: ${outPath} (block size ${blockSize})`);
}

const wizInput = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front.png';
const wizPng = PNG.sync.read(fs.readFileSync(wizInput));
for (const blockSize of [3, 4, 5]) {
  const pix = pixelate(wizPng, blockSize, 6);
  const outPath = `/mnt/k/unityprojects/shoot1/shoot1/scratch/test_wizard_block${blockSize}.png`;
  fs.writeFileSync(outPath, PNG.sync.write(pix));
  console.log(`Saved: ${outPath} (block size ${blockSize})`);
}

const ranInput = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Ranger/ranger_front.png';
const ranPng = PNG.sync.read(fs.readFileSync(ranInput));
for (const blockSize of [3, 4, 5]) {
  const pix = pixelate(ranPng, blockSize, 6);
  const outPath = `/mnt/k/unityprojects/shoot1/shoot1/scratch/test_ranger_block${blockSize}.png`;
  fs.writeFileSync(outPath, PNG.sync.write(pix));
  console.log(`Saved: ${outPath} (block size ${blockSize})`);
}

const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

// Remove tiny floating dust/noise islands
function removeIsolatedIslands(pngData, width, height, minKeepRatio = 0.05) {
  const visited = new Uint8Array(width * height);
  const components = [];

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const idx = y * width + x;
      if (visited[idx] || pngData[(idx << 2) + 3] === 0) continue;

      const queue = [idx];
      visited[idx] = 1;
      let head = 0;

      while (head < queue.length) {
        const curr = queue[head++];
        const cx = curr % width;
        const cy = Math.floor(curr / width);

        for (let dy = -1; dy <= 1; dy++) {
          for (let dx = -1; dx <= 1; dx++) {
            if (dx === 0 && dy === 0) continue;
            const nx = cx + dx;
            const ny = cy + dy;
            if (nx >= 0 && nx < width && ny >= 0 && ny < height) {
              const nIdx = ny * width + nx;
              if (!visited[nIdx] && pngData[(nIdx << 2) + 3] > 0) {
                visited[nIdx] = 1;
                queue.push(nIdx);
              }
            }
          }
        }
      }

      components.push(queue);
    }
  }

  if (components.length <= 1) return;

  components.sort((a, b) => b.length - a.length);
  const mainCompSize = components[0].length;

  for (let i = 1; i < components.length; i++) {
    const comp = components[i];
    if (comp.length < mainCompSize * minKeepRatio && comp.length < 50) {
      for (const pIdx of comp) {
        const dIdx = pIdx << 2;
        pngData[dIdx] = 0;
        pngData[dIdx + 1] = 0;
        pngData[dIdx + 2] = 0;
        pngData[dIdx + 3] = 0;
      }
    }
  }
}

// Crisp Pixel Art Downsample & Nearest-Neighbor Fill
function pixelateImage(srcPng, pixelSize = 3, colorStep = 6) {
  const w = srcPng.width;
  const h = srcPng.height;
  const outPng = new PNG({ width: w, height: h });

  for (let i = 0; i < w * h * 4; i++) outPng.data[i] = 0;

  for (let by = 0; by < h; by += pixelSize) {
    for (let bx = 0; bx < w; bx += pixelSize) {
      let rSum = 0, gSum = 0, bSum = 0, weightSum = 0;
      let totalSamples = 0;
      let opaqueSamples = 0;

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

      // Threshold: if at least ~30% of block is non-transparent, render as solid pixel
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

  removeIsolatedIslands(outPng.data, w, h);
  return outPng;
}

const characters = ['Warrior', 'Ranger', 'Wizard'];
const directions = ['front', 'front_diagonal', 'side', 'back_diagonal', 'back'];
const BLOCK_SIZE = 6;
const COLOR_STEP = 8;

console.log(`Starting Character Pixel Art Conversion with BlockSize=${BLOCK_SIZE}, ColorStep=${COLOR_STEP}...`);

const baseDir = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters';
const backupDir = '/mnt/k/unityprojects/shoot1/shoot1/scratch/original_smooth_sprites';
let totalProcessed = 0;

for (const charName of characters) {
  const prefix = charName.toLowerCase();
  const dirPath = path.join(baseDir, charName);
  const srcDirPath = path.join(backupDir, charName);

  // 1. Process 5 directional sprites
  for (const dir of directions) {
    const fileName = `${prefix}_${dir}.png`;
    const srcFilePath = path.join(srcDirPath, fileName);
    const destFilePath = path.join(dirPath, fileName);
    if (!fs.existsSync(srcFilePath)) {
      console.warn(`Source file not found: ${srcFilePath}`);
      continue;
    }

    const srcPng = PNG.sync.read(fs.readFileSync(srcFilePath));
    const dotPng = pixelateImage(srcPng, BLOCK_SIZE, COLOR_STEP);
    fs.writeFileSync(destFilePath, PNG.sync.write(dotPng));
    console.log(`[OK] Converted to Dot: ${charName}/${fileName} (${dotPng.width}x${dotPng.height})`);
    totalProcessed++;
  }

  // 2. Also process master sheet if exists
  const sheetName = `${prefix}.png`;
  const srcSheetPath = path.join(srcDirPath, sheetName);
  const destSheetPath = path.join(dirPath, sheetName);
  if (fs.existsSync(srcSheetPath)) {
    const srcPng = PNG.sync.read(fs.readFileSync(srcSheetPath));
    const dotPng = pixelateImage(srcPng, BLOCK_SIZE, COLOR_STEP);
    fs.writeFileSync(destSheetPath, PNG.sync.write(dotPng));
    console.log(`[OK] Converted to Dot Sheet: ${charName}/${sheetName} (${dotPng.width}x${dotPng.height})`);
    totalProcessed++;
  }
}

console.log(`\nSuccessfully converted all ${totalProcessed} character sprites to crisp Pixel Art!`);

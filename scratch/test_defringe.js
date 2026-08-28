const fs = require('fs');
const { PNG } = require('pngjs');

// Test defringing and smooth alpha feathering
const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard.png';
const testOut = '/mnt/k/unityprojects/shoot1/shoot1/scratch/test_smooth_wizard.png';

fs.createReadStream(inputPath)
  .pipe(new PNG())
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    // 1. Initial Flood fill for pure background (>= 245)
    const bgMask = new Uint8Array(width * height);
    const queue = [];

    function isPureBg(r, g, b) {
      return r >= 240 && g >= 240 && b >= 240;
    }

    function enqueue(x, y) {
      const idx = y * width + x;
      if (bgMask[idx]) return;
      const pIdx = idx << 2;
      if (isPureBg(data[pIdx], data[pIdx + 1], data[pIdx + 2])) {
        bgMask[idx] = 1;
        queue.push(idx);
      }
    }

    for (let x = 0; x < width; x++) { enqueue(x, 0); enqueue(x, height - 1); }
    for (let y = 0; y < height; y++) { enqueue(0, y); enqueue(width - 1, y); }

    let head = 0;
    while (head < queue.length) {
      const curr = queue[head++];
      const cx = curr % width;
      const cy = Math.floor(curr / width);

      if (cx > 0) check(cx - 1, cy);
      if (cx < width - 1) check(cx + 1, cy);
      if (cy > 0) check(cx, cy - 1);
      if (cy < height - 1) check(cx, cy + 1);
    }

    function check(nx, ny) {
      const nIdx = ny * width + nx;
      if (bgMask[nIdx]) return;
      const npIdx = nIdx << 2;
      if (isPureBg(data[npIdx], data[npIdx + 1], data[npIdx + 2])) {
        bgMask[nIdx] = 1;
        queue.push(nIdx);
      }
    }

    // 2. Multi-level feathering & defringing on border pixels
    // Calculate distance to background or blend alpha based on brightness
    // Characters have dark outlines (RGB < 80).
    // Transitional pixels between outline and pure white bg have RGB between 80 and 240.
    const alphaMap = new Float32Array(width * height);
    for (let i = 0; i < width * height; i++) {
      if (bgMask[i]) {
        alphaMap[i] = 0;
      } else {
        alphaMap[i] = 1.0;
      }
    }

    // Identify border pixels: non-bg pixels that touch bgMask
    // We can calculate alpha as:
    // If adjacent to background, calculate alpha from outline darkness:
    // Outline color is dark (lum ~30..60), bg is pure white (lum ~255).
    // alpha = clamp((240 - lum) / (240 - 70), 0, 1)
    for (let y = 1; y < height - 1; y++) {
      for (let x = 1; x < width - 1; x++) {
        const idx = y * width + x;
        if (bgMask[idx]) continue;

        // Check if touches bgMask within 2 pixels
        let touchesBg = false;
        for (let dy = -2; dy <= 2; dy++) {
          for (let dx = -2; dx <= 2; dx++) {
            if (bgMask[(y + dy) * width + (x + dx)]) {
              touchesBg = true;
              break;
            }
          }
          if (touchesBg) break;
        }

        if (touchesBg) {
          const pIdx = idx << 2;
          const r = data[pIdx], g = data[pIdx + 1], b = data[pIdx + 2];
          const lum = 0.299 * r + 0.587 * g + 0.114 * b;

          if (lum >= 238) {
            alphaMap[idx] = 0;
          } else if (lum > 70) {
            // Smooth falloff from lum 70 (alpha 1) to lum 238 (alpha 0)
            const t = (238 - lum) / (238 - 70);
            alphaMap[idx] = Math.pow(t, 1.2); // smooth gamma

            // DEFRINGE: remove white halo from RGB by dividing out the blended white background
            // C_observed = C_foreground * a + 255 * (1 - a)
            // C_foreground = (C_observed - 255 * (1 - a)) / a
            const a = alphaMap[idx];
            if (a > 0.05) {
              const defringeR = Math.max(0, Math.min(255, Math.round((r - 255 * (1 - a)) / a)));
              const defringeG = Math.max(0, Math.min(255, Math.round((g - 255 * (1 - a)) / a)));
              const defringeB = Math.max(0, Math.min(255, Math.round((b - 255 * (1 - a)) / a)));
              data[pIdx] = defringeR;
              data[pIdx + 1] = defringeG;
              data[pIdx + 2] = defringeB;
            }
          }
        }
      }
    }

    // Apply alpha
    for (let i = 0; i < width * height; i++) {
      const pIdx = i << 2;
      data[pIdx + 3] = Math.round(alphaMap[i] * 255);
    }

    // Crop FRONT wizard
    const targetW = 350, targetH = 450;
    const outPng = new PNG({ width: targetW, height: targetH });
    for (let i = 0; i < targetW * targetH * 4; i++) outPng.data[i] = 0;

    // Bbox for wizard front: [69, 130 - 359, 498]
    const bMinX = 69, bMaxX = 359, bMinY = 130, bMaxY = 498;
    const charW = bMaxX - bMinX + 1;
    const charH = bMaxY - bMinY + 1;
    const destXOffset = Math.floor((targetW - charW) / 2);
    const destYOffset = targetH - 20 - charH;

    for (let sy = bMinY; sy <= bMaxY; sy++) {
      for (let sx = bMinX; sx <= bMaxX; sx++) {
        const srcIdx = (sy * width + sx) << 2;
        const dx = destXOffset + (sx - bMinX);
        const dy = destYOffset + (sy - bMinY);
        if (dx >= 0 && dx < targetW && dy >= 0 && dy < targetH) {
          const destIdx = (dy * targetW + dx) << 2;
          outPng.data[destIdx] = data[srcIdx];
          outPng.data[destIdx + 1] = data[srcIdx + 1];
          outPng.data[destIdx + 2] = data[srcIdx + 2];
          outPng.data[destIdx + 3] = data[srcIdx + 3];
        }
      }
    }

    fs.writeFileSync(testOut, PNG.sync.write(outPng));
    console.log('Saved test defringed wizard:', testOut);
  });

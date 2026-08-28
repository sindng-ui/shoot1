const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

function processSheet({ name, inputPath, outputDir, prefix, bgThreshold, regions }) {
  console.log(`\n========================================`);
  console.log(`Processing [${name}] from ${inputPath}`);
  console.log(`========================================`);

  const fileData = fs.readFileSync(inputPath);
  const png = PNG.sync.read(fileData);
  const width = png.width;
  const height = png.height;
  const data = png.data;

  // 1. Flood Fill for outer background
  const bgMask = new Uint8Array(width * height);
  const queue = [];

  function isBgColor(r, g, b) {
    return r >= bgThreshold && g >= bgThreshold && b >= bgThreshold;
  }

  function enqueue(x, y) {
    const idx = y * width + x;
    if (bgMask[idx]) return;
    const pIdx = idx << 2;
    if (isBgColor(data[pIdx], data[pIdx + 1], data[pIdx + 2])) {
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
    if (isBgColor(data[npIdx], data[npIdx + 1], data[npIdx + 2])) {
      bgMask[nIdx] = 1;
      queue.push(nIdx);
    }
  }

  console.log(`Flood fill removed: ${head} pixels.`);

  // 2. Anti-Aliasing & Defringing border pixels
  const alphaMap = new Float32Array(width * height);
  for (let i = 0; i < width * height; i++) {
    alphaMap[i] = bgMask[i] ? 0 : 1.0;
  }

  const darkThreshold = 65;
  const lightThreshold = Math.min(240, bgThreshold + 5);

  for (let y = 1; y < height - 1; y++) {
    for (let x = 1; x < width - 1; x++) {
      const idx = y * width + x;
      if (bgMask[idx]) continue;

      let touchesBg = false;
      for (let dy = -2; dy <= 2; dy++) {
        for (let dx = -2; dx <= 2; dx++) {
          const ny = y + dy;
          const nx = x + dx;
          if (ny >= 0 && ny < height && nx >= 0 && nx < width) {
            if (bgMask[ny * width + nx]) {
              touchesBg = true;
              break;
            }
          }
        }
        if (touchesBg) break;
      }

      if (touchesBg) {
        const pIdx = idx << 2;
        const r = data[pIdx], g = data[pIdx + 1], b = data[pIdx + 2];
        const lum = 0.299 * r + 0.587 * g + 0.114 * b;

        if (lum >= lightThreshold) {
          alphaMap[idx] = 0;
        } else if (lum > darkThreshold) {
          const t = (lightThreshold - lum) / (lightThreshold - darkThreshold);
          const a = Math.pow(t, 1.25);
          alphaMap[idx] = a;

          // Defringe: subtract out the white background blending
          if (a > 0.08) {
            data[pIdx]     = Math.max(0, Math.min(255, Math.round((r - 255 * (1 - a)) / a)));
            data[pIdx + 1] = Math.max(0, Math.min(255, Math.round((g - 255 * (1 - a)) / a)));
            data[pIdx + 2] = Math.max(0, Math.min(255, Math.round((b - 255 * (1 - a)) / a)));
          }
        }
      }
    }
  }

  // Apply alpha back to data
  for (let i = 0; i < width * height; i++) {
    data[(i << 2) + 3] = Math.round(alphaMap[i] * 255);
  }

  // 3. Slice each region
  const targetW = 350;
  const targetH = 450;
  const bottomPadding = 20;

  for (const reg of regions) {
    let bMinX = width, bMaxX = 0, bMinY = height, bMaxY = 0;
    for (let y = reg.yMin; y < reg.yMax; y++) {
      for (let x = reg.xMin; x < reg.xMax; x++) {
        const pIdx = (y * width + x) << 2;
        if (data[pIdx + 3] > 0) {
          if (x < bMinX) bMinX = x;
          if (x > bMaxX) bMaxX = x;
          if (y < bMinY) bMinY = y;
          if (y > bMaxY) bMaxY = y;
        }
      }
    }

    const charW = bMaxX - bMinX + 1;
    const charH = bMaxY - bMinY + 1;
    console.log(`[${reg.name}] size: ${charW}x${charH}, bbox: [${bMinX},${bMinY} - ${bMaxX},${bMaxY}]`);

    const outPng = new PNG({ width: targetW, height: targetH });
    for (let i = 0; i < targetW * targetH * 4; i++) outPng.data[i] = 0;

    const destXOffset = Math.floor((targetW - charW) / 2);
    const destYOffset = targetH - bottomPadding - charH;
    const shouldFlipX = (reg.name === `${prefix}_back_diagonal`);

    for (let sy = bMinY; sy <= bMaxY; sy++) {
      for (let sx = bMinX; sx <= bMaxX; sx++) {
        const srcIdx = (sy * width + sx) << 2;
        const localX = shouldFlipX ? (charW - 1 - (sx - bMinX)) : (sx - bMinX);
        const dx = destXOffset + localX;
        const dy = destYOffset + (sy - bMinY);

        if (dx >= 0 && dx < targetW && dy >= 0 && dy < targetH) {
          const destIdx = (dy * targetW + dx) << 2;
          outPng.data[destIdx]     = data[srcIdx];
          outPng.data[destIdx + 1] = data[srcIdx + 1];
          outPng.data[destIdx + 2] = data[srcIdx + 2];
          outPng.data[destIdx + 3] = data[srcIdx + 3];
        }
      }
    }

    const outPath = path.join(outputDir, `${reg.name}.png`);
    fs.writeFileSync(outPath, PNG.sync.write(outPng));
    console.log(`  -> Generated Smooth & Defringed: ${outPath}`);
  }
}

// 1. Warrior
processSheet({
  name: 'Warrior',
  inputPath: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior.png',
  outputDir: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior',
  prefix: 'warrior',
  bgThreshold: 235,
  regions: [
    { name: 'warrior_front',          xMin: 0,   xMax: 340,  yMin: 0,   yMax: 512 },
    { name: 'warrior_front_diagonal', xMin: 340, xMax: 680,  yMin: 0,   yMax: 512 },
    { name: 'warrior_side',           xMin: 680, xMax: 1024, yMin: 0,   yMax: 512 },
    { name: 'warrior_back_diagonal',  xMin: 170, xMax: 512,  yMin: 512, yMax: 1024 },
    { name: 'warrior_back',           xMin: 512, xMax: 850,  yMin: 512, yMax: 1024 }
  ]
});

// 2. Ranger (bgThreshold 195 to also erase light-grey floor shadow oval and text labels)
processSheet({
  name: 'Ranger',
  inputPath: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Ranger/ranger.png',
  outputDir: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Ranger',
  prefix: 'ranger',
  bgThreshold: 195,
  regions: [
    { name: 'ranger_front',          xMin: 30,  xMax: 340, yMin: 40,  yMax: 410 },
    { name: 'ranger_front_diagonal', xMin: 360, xMax: 660, yMin: 40,  yMax: 410 },
    { name: 'ranger_side',           xMin: 680, xMax: 990, yMin: 40,  yMax: 410 },
    { name: 'ranger_back_diagonal',  xMin: 360, xMax: 660, yMin: 500, yMax: 880 },
    { name: 'ranger_back',           xMin: 680, xMax: 990, yMin: 500, yMax: 880 }
  ]
});

// 3. Wizard
processSheet({
  name: 'Wizard',
  inputPath: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard.png',
  outputDir: '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard',
  prefix: 'wizard',
  bgThreshold: 235,
  regions: [
    { name: 'wizard_front',          xMin: 20,  xMax: 360,  yMin: 50,  yMax: 512 },
    { name: 'wizard_front_diagonal', xMin: 360, xMax: 680,  yMin: 50,  yMax: 512 },
    { name: 'wizard_side',           xMin: 680, xMax: 1000, yMin: 50,  yMax: 512 },
    { name: 'wizard_back_diagonal',  xMin: 180, xMax: 490,  yMin: 500, yMax: 950 },
    { name: 'wizard_back',           xMin: 490, xMax: 840,  yMin: 500, yMax: 950 }
  ]
});

console.log('\nAll 15 smooth defringed hero sprites created successfully!');

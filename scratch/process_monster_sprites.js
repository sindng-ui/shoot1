const fs = require('fs');
const path = require('path');
const { PNG } = require('./node_modules/pngjs');

const monsters = [
  { name: 'Slime', sub: 'Slime', file: 'slime.png' },
  { name: 'VampireBat', sub: 'VampireBat', file: 'vampirebat.png' },
  { name: 'Skeleton', sub: 'Skeleton', file: 'skeleton.png' },
  { name: 'FireImp', sub: 'FireImp', file: 'fireimp.png' },
  { name: 'ToxicSpider', sub: 'ToxicSpider', file: 'toxicspider.png' },
  { name: 'DarkNight', sub: 'DarkNight', file: 'darknight.png' },
  { name: 'AncientRockGolem', sub: 'AncientRockGolem', file: 'ancientrockgolem.png' },
  { name: 'LichKing', sub: 'LichKing', file: 'lichking.png' }
];

const baseDir = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Monsters';
const rawDir = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Monsters/Raw';

if (!fs.existsSync(rawDir)) {
  fs.mkdirSync(rawDir, { recursive: true });
}

for (const m of monsters) {
  const filePath = path.join(baseDir, m.sub, m.file);
  const rawPath = path.join(rawDir, m.file);

  // Backup raw original if not already backed up
  if (!fs.existsSync(rawPath)) {
    fs.copyFileSync(filePath, rawPath);
    console.log(Backed up raw file to );
  }

  const rawBuf = fs.readFileSync(rawPath);
  const png = PNG.sync.read(rawBuf);
  const w = png.width;
  const h = png.height;
  const d = png.data;

  // 1. BFS Flood fill from outer edges
  const bgMask = new Uint8Array(w * h);
  const queue = [];

  function isBgColor(r, g, b) {
    // Pure or near-white background
    return r >= 235 && g >= 235 && b >= 235;
  }

  function enqueue(x, y) {
    const idx = y * w + x;
    if (bgMask[idx]) return;
    const pIdx = idx << 2;
    if (isBgColor(d[pIdx], d[pIdx+1], d[pIdx+2])) {
      bgMask[idx] = 1;
      queue.push(idx);
    }
  }

  for (let x = 0; x < w; x++) {
    enqueue(x, 0);
    enqueue(x, h - 1);
  }
  for (let y = 0; y < h; y++) {
    enqueue(0, y);
    enqueue(w - 1, y);
  }

  let head = 0;
  while (head < queue.length) {
    const curr = queue[head++];
    const cx = curr % w;
    const cy = Math.floor(curr / w);

    if (cx > 0) checkNeighbor(cx - 1, cy);
    if (cx < w - 1) checkNeighbor(cx + 1, cy);
    if (cy > 0) checkNeighbor(cx, cy - 1);
    if (cy < h - 1) checkNeighbor(cx, cy + 1);
  }

  function checkNeighbor(nx, ny) {
    const nIdx = ny * w + nx;
    if (bgMask[nIdx]) return;
    const npIdx = nIdx << 2;
    if (isBgColor(d[npIdx], d[npIdx+1], d[npIdx+2])) {
      bgMask[nIdx] = 1;
      queue.push(nIdx);
    }
  }

  // 2. Erase background and defringe edge halos
  for (let idx = 0; idx < w * h; idx++) {
    const pIdx = idx << 2;
    if (bgMask[idx] === 1) {
      d[pIdx + 3] = 0; // Completely transparent
    }
  }

  // Defringe: for pixels adjacent to bgMask, if very bright, blend alpha smoothly
  for (let y = 1; y < h - 1; y++) {
    for (let x = 1; x < w - 1; x++) {
      const idx = y * w + x;
      if (bgMask[idx] === 1) continue;

      // Check if neighboring transparent pixel
      const hasBgNeighbor = bgMask[idx - 1] || bgMask[idx + 1] || bgMask[idx - w] || bgMask[idx + w];
      if (hasBgNeighbor) {
        const pIdx = idx << 2;
        const r = d[pIdx];
        const g = d[pIdx+1];
        const b = d[pIdx+2];

        // If light fringe pixel (white aura / edge bleeding)
        const brightness = (r + g + b) / 3;
        if (brightness > 220) {
          // Feather alpha based on brightness
          const factor = (255 - brightness) / 35; // 220 -> 1.0, 255 -> 0.0
          d[pIdx + 3] = Math.max(0, Math.min(255, Math.floor(d[pIdx + 3] * Math.max(0, factor))));
        }
      }
    }
  }

  // 3. Save processed transparent PNG back to original destination
  const processedBuf = PNG.sync.write(png);
  fs.writeFileSync(filePath, processedBuf);
  console.log([SUCCESS] Processed and saved:  (x));
}

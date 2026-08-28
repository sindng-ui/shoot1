const fs = require('fs');
const { PNG } = require('pngjs');

const p = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';
const data = PNG.sync.read(fs.readFileSync(p));
const w = data.width, h = data.height;

// Find bounding boxes of distinct connected components of glove pixels
const gloveMap = new Uint8Array(w * h);
for (let y = 280; y < 400; y++) {
  for (let x = 50; x < 320; x++) {
    const idx = (y * w + x) << 2;
    const a = data.data[idx + 3];
    const r = data.data[idx], g = data.data[idx+1], b = data.data[idx+2];
    if (a > 150 && r >= 30 && r <= 70 && g >= 25 && g <= 65 && b >= 45 && b <= 95 && Math.abs(r-g) < 20) {
      gloveMap[y * w + x] = 1;
    }
  }
}

// Connected components
const visited = new Uint8Array(w * h);
const components = [];
for (let y = 280; y < 400; y++) {
  for (let x = 50; x < 320; x++) {
    const idx = y * w + x;
    if (gloveMap[idx] && !visited[idx]) {
      const comp = [];
      const q = [idx];
      visited[idx] = 1;
      let head = 0;
      while (head < q.length) {
        const curr = q[head++];
        comp.push(curr);
        const cx = curr % w;
        const cy = Math.floor(curr / w);
        for (let dy = -1; dy <= 1; dy++) {
          for (let dx = -1; dx <= 1; dx++) {
            const nx = cx + dx, ny = cy + dy;
            if (nx >= 0 && nx < w && ny >= 0 && ny < h) {
              const nIdx = ny * w + nx;
              if (gloveMap[nIdx] && !visited[nIdx]) {
                visited[nIdx] = 1;
                q.push(nIdx);
              }
            }
          }
        }
      }
      if (comp.length > 20) components.push(comp);
    }
  }
}

console.log(`Found ${components.length} glove components:`);
components.forEach((comp, i) => {
  let minX = w, maxX = 0, minY = h, maxY = 0;
  for (const idx of comp) {
    const x = idx % w, y = Math.floor(idx / w);
    if (x < minX) minX = x; if (x > maxX) maxX = x;
    if (y < minY) minY = y; if (y > maxY) maxY = y;
  }
  const avgX = (minX + maxX) / 2;
  const avgY = (minY + maxY) / 2;
  const wx = ((avgX - 175) / 450 * 1.5).toFixed(3);
  const wy = ((315 - avgY) / 450 * 1.5).toFixed(3);
  console.log(`  Hand #${i+1}: pixels=${comp.length}, bbox=[${minX}..${maxX}, ${minY}..${maxY}], center=(${avgX}, ${avgY}) -> World: (wx=${wx}, wy=${wy})`);
});

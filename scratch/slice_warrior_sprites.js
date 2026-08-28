const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior.png';
const outputDir = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    // 1. BFS Flood fill from edges to erase white background
    const visited = new Uint8Array(width * height);
    const queue = [];

    function isBackground(r, g, b) {
      return r >= 235 && g >= 235 && b >= 235;
    }

    function enqueue(x, y) {
      const idx = y * width + x;
      if (visited[idx]) return;
      const pIdx = idx << 2;
      if (isBackground(data[pIdx], data[pIdx + 1], data[pIdx + 2])) {
        visited[idx] = 1;
        queue.push(idx);
      }
    }

    for (let x = 0; x < width; x++) {
      enqueue(x, 0);
      enqueue(x, height - 1);
    }
    for (let y = 0; y < height; y++) {
      enqueue(0, y);
      enqueue(width - 1, y);
    }

    let head = 0;
    while (head < queue.length) {
      const curr = queue[head++];
      const cx = curr % width;
      const cy = Math.floor(curr / width);

      const pIdx = curr << 2;
      data[pIdx + 3] = 0; // Transparent

      if (cx > 0) checkNeighbor(cx - 1, cy);
      if (cx < width - 1) checkNeighbor(cx + 1, cy);
      if (cy > 0) checkNeighbor(cx, cy - 1);
      if (cy < height - 1) checkNeighbor(cx, cy + 1);
    }

    function checkNeighbor(nx, ny) {
      const nIdx = ny * width + nx;
      if (visited[nIdx]) return;
      const npIdx = nIdx << 2;
      if (isBackground(data[npIdx], data[npIdx + 1], data[npIdx + 2])) {
        visited[nIdx] = 1;
        queue.push(nIdx);
      }
    }

    console.log(`Flood fill background removed: ${head} pixels.`);

    // 2. Define the 5 regions
    const regions = [
      { name: 'warrior_front',          xMin: 0,   xMax: 350, yMin: 0,   yMax: 512 },
      { name: 'warrior_front_diagonal', xMin: 350, xMax: 680, yMin: 0,   yMax: 512 },
      { name: 'warrior_side',           xMin: 680, xMax: 1024,yMin: 0,   yMax: 512 },
      { name: 'warrior_back_diagonal',  xMin: 50,  xMax: 512, yMin: 512, yMax: 1024 },
      { name: 'warrior_back',           xMin: 512, xMax: 950, yMin: 512, yMax: 1024 }
    ];

    // Standard canvas size for sliced sprites
    // Max character width is ~287, height ~419.
    // Standard canvas: 350x450, padding at bottom: 20px
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
      console.log(`Processing ${reg.name}: size ${charW}x${charH}, bbox [${bMinX},${bMinY} - ${bMaxX},${bMaxY}]`);

      // Create output PNG
      const outPng = new PNG({ width: targetW, height: targetH });
      // Fill clear
      for (let i = 0; i < targetW * targetH * 4; i++) outPng.data[i] = 0;

      // Position character horizontally centered, and bottom aligned with bottomPadding
      const destXOffset = Math.floor((targetW - charW) / 2);
      const destYOffset = targetH - bottomPadding - charH; // Y=0 is top in PNG

      const shouldFlipX = (reg.name === 'warrior_back_diagonal');

      for (let sy = bMinY; sy <= bMaxY; sy++) {
        for (let sx = bMinX; sx <= bMaxX; sx++) {
          const srcIdx = (sy * width + sx) << 2;
          const localX = shouldFlipX ? (charW - 1 - (sx - bMinX)) : (sx - bMinX);
          const dx = destXOffset + localX;
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

      const outFile = path.join(outputDir, `${reg.name}.png`);
      const outBuffer = PNG.sync.write(outPng);
      fs.writeFileSync(outFile, outBuffer);
      console.log(`Saved: ${outFile} (${targetW}x${targetH})`);
    }

    console.log('All 5 warrior sprites sliced successfully!');
  });

const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior.png';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    // 1. BFS Flood fill from edges to make background transparent
    const visited = new Uint8Array(width * height);
    const queue = [];

    function isBackground(r, g, b) {
      // White/near white background
      return r >= 238 && g >= 238 && b >= 238;
    }

    // Push all border pixels that are background
    for (let x = 0; x < width; x++) {
      enqueue(x, 0);
      enqueue(x, height - 1);
    }
    for (let y = 0; y < height; y++) {
      enqueue(0, y);
      enqueue(width - 1, y);
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

    let head = 0;
    while (head < queue.length) {
      const curr = queue[head++];
      const cx = curr % width;
      const cy = Math.floor(curr / width);

      // Make transparent
      const pIdx = curr << 2;
      data[pIdx + 3] = 0;

      // 4-way neighbors
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

    console.log(`Flood fill completed. Background pixels erased: ${head}`);

    // Now find bounding boxes for the 5 characters
    // Grid regions:
    // Row 0: 3 characters (Front, FrontDiag, Side)
    // Row 1: 2 characters (BackDiag, Back)
    const regions = [
      { name: 'Front', xMin: 0, xMax: 350, yMin: 0, yMax: 512 },
      { name: 'FrontDiagonal', xMin: 350, xMax: 680, yMin: 0, yMax: 512 },
      { name: 'Side', xMin: 680, xMax: 1024, yMin: 0, yMax: 512 },
      { name: 'BackDiagonal', xMin: 50, xMax: 512, yMin: 512, yMax: 1024 },
      { name: 'Back', xMin: 512, xMax: 950, yMin: 512, yMax: 1024 }
    ];

    for (const reg of regions) {
      let bMinX = width, bMaxX = 0, bMinY = height, bMaxY = 0;
      let count = 0;
      for (let y = reg.yMin; y < reg.yMax; y++) {
        for (let x = reg.xMin; x < reg.xMax; x++) {
          const pIdx = (y * width + x) << 2;
          if (data[pIdx + 3] > 0) {
            count++;
            if (x < bMinX) bMinX = x;
            if (x > bMaxX) bMaxX = x;
            if (y < bMinY) bMinY = y;
            if (y > bMaxY) bMaxY = y;
          }
        }
      }
      console.log(`Region ${reg.name}: pixels=${count}, BBox=[${bMinX}, ${bMinY}, ${bMaxX}, ${bMaxY}], Size=${bMaxX - bMinX + 1}x${bMaxY - bMinY + 1}`);
    }
  });

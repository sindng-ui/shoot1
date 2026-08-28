const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard.png';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    console.log(`Loaded Wizard PNG: ${this.width}x${this.height}`);

    const width = this.width;
    const height = this.height;
    const data = this.data;

    // Row 0 (Y: 0..512):
    // 1. FRONT: X: 0..350
    // 2. FRONT_DIAG: X: 350..680
    // 3. SIDE: X: 680..1024
    // Row 1 (Y: 512..1024):
    // 4. BACK_DIAG: X: 150..500
    // 5. BACK: X: 500..850

    const regions = [
      { name: 'FRONT', xMin: 20, xMax: 360, yMin: 50, yMax: 500 },
      { name: 'FRONT_DIAG', xMin: 360, xMax: 680, yMin: 50, yMax: 500 },
      { name: 'SIDE', xMin: 680, xMax: 1000, yMin: 50, yMax: 500 },
      { name: 'BACK_DIAG', xMin: 180, xMax: 480, yMin: 500, yMax: 950 },
      { name: 'BACK', xMin: 480, xMax: 820, yMin: 500, yMax: 950 }
    ];

    for (const r of regions) {
      let minX = width, maxX = 0, minY = height, maxY = 0;
      let count = 0;
      for (let y = r.yMin; y < r.yMax; y++) {
        for (let x = r.xMin; x < r.xMax; x++) {
          const idx = (y * width + x) << 2;
          const red = data[idx], green = data[idx+1], blue = data[idx+2];
          if (red < 235 || green < 235 || blue < 235) {
            count++;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
          }
        }
      }
      console.log(`Region ${r.name}: count=${count}, bbox=[${minX}, ${minY} - ${maxX}, ${maxY}], size=${maxX - minX + 1}x${maxY - minY + 1}`);
    }
  });

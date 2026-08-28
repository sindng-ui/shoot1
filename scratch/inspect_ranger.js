const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Ranger/ranger.png';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    console.log(`Loaded Ranger PNG: ${this.width}x${this.height}`);

    const width = this.width;
    const height = this.height;
    const data = this.data;

    // Regions:
    // Row 0 (Y: 0..480):
    // 1. FRONT: X: 30..330
    // 2. 3/4 FRONT: X: 360..660
    // 3. SIDE: X: 690..980
    // Row 1 (Y: 480..960):
    // 4. 3/4 BACK: X: 360..660
    // 5. BACK: X: 690..980

    const regions = [
      { name: 'FRONT', xMin: 30, xMax: 330, yMin: 50, yMax: 430 },
      { name: 'FRONT_DIAG', xMin: 360, xMax: 660, yMin: 50, yMax: 430 },
      { name: 'SIDE', xMin: 690, xMax: 980, yMin: 50, yMax: 430 },
      { name: 'BACK_DIAG', xMin: 360, xMax: 660, yMin: 500, yMax: 890 },
      { name: 'BACK', xMin: 690, xMax: 980, yMin: 500, yMax: 890 }
    ];

    for (const r of regions) {
      console.log(`Analyzing region ${r.name}...`);
      // Find non-white pixels
      let minX = width, maxX = 0, minY = height, maxY = 0;
      let nonWhiteCount = 0;
      for (let y = r.yMin; y < r.yMax; y++) {
        for (let x = r.xMin; x < r.xMax; x++) {
          const idx = (y * width + x) << 2;
          const red = data[idx], green = data[idx+1], blue = data[idx+2];
          // Check if not white (say < 235)
          if (red < 235 || green < 235 || blue < 235) {
            nonWhiteCount++;
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
          }
        }
      }
      console.log(`Region ${r.name}: count=${nonWhiteCount}, bbox=[${minX}, ${minY} - ${maxX}, ${maxY}], size=${maxX - minX + 1}x${maxY - minY + 1}`);
    }
  });

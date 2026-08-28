const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front.png';

fs.createReadStream(inputPath)
  .pipe(new PNG())
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    console.log(`Checking wizard_front.png (${width}x${height}):`);
    let jaggedCount = 0;
    // Find border pixels (alpha > 0 but adjacent to alpha == 0)
    for (let y = 1; y < height - 1; y++) {
      for (let x = 1; x < width - 1; x++) {
        const idx = (y * width + x) << 2;
        if (data[idx + 3] > 0) {
          // Check if any neighbor is transparent
          const n1 = ((y - 1) * width + x) << 2;
          const n2 = ((y + 1) * width + x) << 2;
          const n3 = (y * width + (x - 1)) << 2;
          const n4 = (y * width + (x + 1)) << 2;
          if (data[n1 + 3] === 0 || data[n2 + 3] === 0 || data[n3 + 3] === 0 || data[n4 + 3] === 0) {
            jaggedCount++;
            if (jaggedCount <= 10) {
              console.log(`Border pixel at (${x},${y}): RGBA=[${data[idx]}, ${data[idx+1]}, ${data[idx+2]}, ${data[idx+3]}]`);
            }
          }
        }
      }
    }
    console.log(`Total border pixels: ${jaggedCount}`);
  });

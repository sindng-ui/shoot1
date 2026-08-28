const fs = require('fs');
const { PNG } = require('pngjs');

const imgPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';

fs.createReadStream(imgPath)
  .pipe(new PNG())
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    console.log(`Checking wizard_front_diagonal.png (${width}x${height}):`);
    // Find all opaque pixels in right margin (X > 280)
    for (let y = 0; y < height; y++) {
      for (let x = 250; x < width; x++) {
        const idx = (y * width + x) << 2;
        const a = data[idx + 3];
        if (a > 30) {
          console.log(`Pixel at (${x}, ${y}): RGBA=[${data[idx]}, ${data[idx+1]}, ${data[idx+2]}, ${a}]`);
        }
      }
    }
  });

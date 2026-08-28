const fs = require('fs');
const { PNG } = require('pngjs');

const imgPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';

fs.createReadStream(imgPath)
  .pipe(new PNG())
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    let maxX = 0;
    for (let y = 0; y < height; y++) {
      for (let x = 0; x < width; x++) {
        const idx = (y * width + x) << 2;
        if (data[idx + 3] > 0) {
          if (x > maxX) maxX = x;
        }
      }
    }
    console.log(`wizard_front_diagonal maxX = ${maxX} (canvas width = ${width})`);
  });

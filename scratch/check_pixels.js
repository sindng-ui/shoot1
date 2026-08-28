const fs = require('fs');
const { PNG } = require('pngjs');

const imgPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';

fs.createReadStream(imgPath)
  .pipe(new PNG())
  .on('parsed', function() {
    const width = this.width;
    const height = this.height;
    const data = this.data;

    for (let y = 320; y < 420; y++) {
      for (let x = 280; x < width; x++) {
        const idx = (y * width + x) << 2;
        if (data[idx + 3] > 0) {
          console.log(`(${x}, ${y}): RGBA=[${data[idx]}, ${data[idx+1]}, ${data[idx+2]}, ${data[idx+3]}]`);
        }
      }
    }
  });

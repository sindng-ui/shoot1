const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Ranger/ranger.png';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    const width = this.width;
    const data = this.data;

    // Check bottom of FRONT character around Y: 390..440, X: 160 (center)
    console.log('FRONT Vertical strip around feet:');
    for (let y = 380; y <= 440; y += 2) {
      const idx = (y * width + 160) << 2;
      console.log(`Y=${y}: [${data[idx]}, ${data[idx+1]}, ${data[idx+2]}]`);
    }
  });

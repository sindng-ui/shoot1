const fs = require('fs');
const { PNG } = require('pngjs');

const inputPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Warrior/warrior.png';

fs.createReadStream(inputPath)
  .pipe(new PNG({ filterType: 4 }))
  .on('parsed', function() {
    console.log(`Loaded PNG: ${this.width}x${this.height}`);
    
    // Check corners color
    function getPixel(x, y, data, width) {
      const idx = (width * y + x) << 2;
      return [data[idx], data[idx + 1], data[idx + 2], data[idx + 3]];
    }

    console.log('Top-left:', getPixel(0, 0, this.data, this.width));
    console.log('Top-right:', getPixel(this.width - 1, 0, this.data, this.width));
    console.log('Bottom-left:', getPixel(0, this.height - 1, this.data, this.width));
    console.log('Bottom-right:', getPixel(this.width - 1, this.height - 1, this.data, this.width));
  });

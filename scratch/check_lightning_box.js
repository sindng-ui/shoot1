const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const imgPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
const buf = fs.readFileSync(imgPath);
const png = PNG.sync.read(buf);

let minX = png.width, maxX = 0, minY = png.height, maxY = 0;

for (let y = 0; y < png.height; y++) {
    for (let x = 0; x < png.width; x++) {
        let idx = (y * png.width + x) * 4;
        let r = png.data[idx];
        let g = png.data[idx+1];
        let b = png.data[idx+2];
        let brightness = Math.max(r, g, b);
        if (brightness > 35) {
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
    }
}

console.log(`Bounding Box of bright lightning: X [${minX} ~ ${maxX}] (width: ${maxX - minX + 1}), Y [${minY} ~ ${maxY}] (height: ${maxY - minY + 1})`);

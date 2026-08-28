const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const imgPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
if (!fs.existsSync(imgPath)) {
    console.error("File not found:", imgPath);
    process.exit(1);
}

const buf = fs.readFileSync(imgPath);
const png = PNG.sync.read(buf);
console.log("Image size:", png.width, "x", png.height);

let minA = 255, maxA = 0;
let darkCount = 0;
let transparentCount = 0;
let brightCount = 0;

for (let i = 0; i < png.data.length; i += 4) {
    let r = png.data[i];
    let g = png.data[i+1];
    let b = png.data[i+2];
    let a = png.data[i+3];

    if (a < minA) minA = a;
    if (a > maxA) maxA = a;
    if (a < 20) transparentCount++;
    else if (r < 25 && g < 25 && b < 25) darkCount++;
    else brightCount++;
}

console.log(`Alpha range: ${minA} ~ ${maxA}`);
console.log(`Transparent pixels (<20): ${transparentCount}`);
console.log(`Dark pixels (<25 RGB): ${darkCount}`);
console.log(`Bright pixels: ${brightCount}`);

function getPixel(x, y) {
    let idx = (y * png.width + x) * 4;
    return `RGBA(${png.data[idx]}, ${png.data[idx+1]}, ${png.data[idx+2]}, ${png.data[idx+3]})`;
}

console.log("Top-Left (0,0):", getPixel(0, 0));
console.log("Top-Right (w-1,0):", getPixel(png.width - 1, 0));
console.log("Bottom-Left (0,h-1):", getPixel(0, png.height - 1));
console.log("Center:", getPixel(Math.floor(png.width/2), Math.floor(png.height/2)));

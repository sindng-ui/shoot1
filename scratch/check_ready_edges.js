const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const filePath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning_ready.png");
const buf = fs.readFileSync(filePath);
const png = PNG.sync.read(buf);

console.log("Size:", png.width, "x", png.height);

// Check alpha at edges (X = 0, X = width-1)
let leftEdgeMaxA = 0;
let rightEdgeMaxA = 0;

for (let y = 0; y < png.height; y++) {
    let leftA = png.data[(y * png.width + 0) * 4 + 3];
    let rightA = png.data[(y * png.width + (png.width - 1)) * 4 + 3];
    if (leftA > leftEdgeMaxA) leftEdgeMaxA = leftA;
    if (rightA > rightEdgeMaxA) rightEdgeMaxA = rightA;
}

console.log(`Left edge max alpha: ${leftEdgeMaxA}, Right edge max alpha: ${rightEdgeMaxA}`);

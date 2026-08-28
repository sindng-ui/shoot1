const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const filePath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning_ready.png");
const buf = fs.readFileSync(filePath);
const png = PNG.sync.read(buf);

let topEdgeMaxA = 0;
let botEdgeMaxA = 0;

for (let x = 0; x < png.width; x++) {
    let topA = png.data[(0 * png.width + x) * 4 + 3];
    let botA = png.data[((png.height - 1) * png.width + x) * 4 + 3];
    if (topA > topEdgeMaxA) topEdgeMaxA = topA;
    if (botA > botEdgeMaxA) botEdgeMaxA = botA;
}

console.log(`Top edge max alpha: ${topEdgeMaxA}, Bottom edge max alpha: ${botEdgeMaxA}`);

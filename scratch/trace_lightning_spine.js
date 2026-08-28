const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const rawPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
const buf = fs.readFileSync(rawPath);
const png = PNG.sync.read(buf);

console.log("Raw size:", png.width, "x", png.height);

// Track the center line Y of the brightest pixel at each X
let lineY = [];
for (let x = 0; x < png.width; x += 16) {
    let maxB = 0;
    let bestY = 0;
    for (let y = 0; y < png.height; y++) {
        let idx = (y * png.width + x) * 4;
        let b = Math.max(png.data[idx], png.data[idx+1], png.data[idx+2]);
        if (b > maxB) {
            maxB = b;
            bestY = y;
        }
    }
    lineY.push({ x, y: bestY, maxB });
}

let minY = Math.min(...lineY.filter(p => p.maxB > 50).map(p => p.y));
let maxY = Math.max(...lineY.filter(p => p.maxB > 50).map(p => p.y));
console.log(`Core line Y variation across X: min ${minY}, max ${maxY}, amplitude: ${maxY - minY}px`);

// Sample 10 points along the length
console.log("Sample points (x -> y):");
lineY.filter((_, idx) => idx % 6 === 0).forEach(p => {
    console.log(`X: ${p.x} -> Y: ${p.y} (brightness: ${p.maxB})`);
});

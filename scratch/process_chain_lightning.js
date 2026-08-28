const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const srcPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
const destPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning_ready.png");

const buf = fs.readFileSync(srcPath);
const srcPng = PNG.sync.read(buf);

const width = srcPng.width;
const height = srcPng.height;

// Find weighted vertical center
let totalWeight = 0;
let weightedYSum = 0;
let minY = height, maxY = 0;

for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
        let idx = (y * width + x) * 4;
        let r = srcPng.data[idx];
        let g = srcPng.data[idx+1];
        let b = srcPng.data[idx+2];
        let br = Math.max(r, g, b);
        if (br > 30) {
            totalWeight += br;
            weightedYSum += y * br;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
    }
}

const centerY = Math.round(weightedYSum / totalWeight);
console.log(`Lightning Y range: [${minY} ~ ${maxY}], Height: ${maxY - minY + 1}, Weighted Center Y: ${centerY}`);

// Symmetrical height crop around centerY
const halfHeight = Math.max(centerY - minY, maxY - centerY) + 24;
const startY = Math.max(0, centerY - halfHeight);
const endY = Math.min(height, startY + halfHeight * 2);
const actualHeight = endY - startY;

console.log(`Cropping from Y: ${startY} to ${endY} (height: ${actualHeight})`);

const outPng = new PNG({ width: width, height: actualHeight });

// Horizontal fade margin in pixels
const fadeMargin = 48;

for (let y = 0; y < actualHeight; y++) {
    let srcY = startY + y;
    for (let x = 0; x < width; x++) {
        let srcIdx = (srcY * width + x) * 4;
        let outIdx = (y * width + x) * 4;

        let r = srcPng.data[srcIdx];
        let g = srcPng.data[srcIdx+1];
        let b = srcPng.data[srcIdx+2];

        let maxC = Math.max(r, g, b);

        let alpha = 0;
        if (maxC > 12) {
            let t = (maxC - 12) / (255 - 12);
            alpha = Math.min(255, Math.round(Math.pow(t, 0.75) * 255));
        }

        // Apply smooth horizontal edge fade
        let xFactor = 1.0;
        if (x < fadeMargin) {
            let ft = x / fadeMargin;
            xFactor = ft * ft * (3 - 2 * ft); // smoothstep
        } else if (x > width - 1 - fadeMargin) {
            let ft = (width - 1 - x) / fadeMargin;
            xFactor = ft * ft * (3 - 2 * ft);
        }

        alpha = Math.round(alpha * xFactor);

        if (alpha <= 0) {
            outPng.data[outIdx] = 0;
            outPng.data[outIdx+1] = 0;
            outPng.data[outIdx+2] = 0;
            outPng.data[outIdx+3] = 0;
        } else {
            let aFactor = alpha / 255;
            let boost = 1.05;
            let unR = Math.min(255, Math.round((r / (maxC / 255)) * boost));
            let unG = Math.min(255, Math.round((g / (maxC / 255)) * boost));
            let unB = Math.min(255, Math.round((b / (maxC / 255)) * boost));

            outPng.data[outIdx] = unR;
            outPng.data[outIdx+1] = unG;
            outPng.data[outIdx+2] = unB;
            outPng.data[outIdx+3] = alpha;
        }
    }
}

fs.writeFileSync(destPath, PNG.sync.write(outPng));
console.log(`Saved polished lightning sprite: ${destPath} (${width}x${actualHeight})`);

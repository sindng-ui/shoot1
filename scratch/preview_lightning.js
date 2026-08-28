const fs = require("fs");
const path = require("path");
const { PNG } = require("./node_modules/pngjs");

const imgPath = path.resolve(__dirname, "../Assets/Resources/skills/ChainLightning/chainlightning.png");
const buf = fs.readFileSync(imgPath);
const png = PNG.sync.read(buf);

// Sample down to 80x25 characters
const cols = 80;
const rows = 24;
let grid = Array(rows).fill(0).map(() => Array(cols).fill(" "));

for (let r = 0; r < rows; r++) {
    for (let c = 0; c < cols; c++) {
        let startX = Math.floor(c * png.width / cols);
        let endX = Math.floor((c + 1) * png.width / cols);
        let startY = Math.floor(r * png.height / rows);
        let endY = Math.floor((r + 1) * png.height / rows);

        let maxB = 0;
        for (let y = startY; y < endY; y += 2) {
            for (let x = startX; x < endX; x += 2) {
                let idx = (y * png.width + x) * 4;
                let b = Math.max(png.data[idx], png.data[idx+1], png.data[idx+2]);
                if (b > maxB) maxB = b;
            }
        }

        if (maxB > 220) grid[r][c] = "#";
        else if (maxB > 150) grid[r][c] = "*";
        else if (maxB > 80) grid[r][c] = ".";
        else grid[r][c] = " ";
    }
}

console.log(grid.map(row => row.join("")).join("\n"));


const fs = require("fs");
const { PNG } = require("pngjs");

const buf = fs.readFileSync("/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Monsters/Skeleton/skeleton.png");
const png = PNG.sync.read(buf);

let left = 0, right = 0;
let mid = Math.floor(png.width / 2);

for (let y = 0; y < png.height; y++) {
  for (let x = 0; x < png.width; x++) {
    const a = png.data[(y * png.width + x) * 4 + 3];
    if (a > 50) {
      if (x < mid) left++;
      else right++;
    }
  }
}
console.log("Skeleton pixel balance: Left =", left, "Right =", right);

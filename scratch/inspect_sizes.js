const fs = require('fs');
const path = require('path');
const { PNG } = require('pngjs');

const files = [
  "Assets/Resources/Characters/Warrior/warrior_front.png",
  "Assets/Resources/Characters/Warrior/warrior_side.png",
  "Assets/Resources/Characters/Ranger/ranger_front.png",
  "Assets/Resources/Characters/Ranger/ranger_side.png",
  "Assets/Resources/Characters/Wizard/wizard_front.png",
  "Assets/Resources/Characters/Wizard/wizard_side.png"
];

for (const rel of files) {
  const full = path.resolve(__dirname, '..', rel);
  if (!fs.existsSync(full)) {
    console.log('Not found:', full);
    continue;
  }
  const buf = fs.readFileSync(full);
  const p = PNG.sync.read(buf);
  let minX = p.width, maxX = 0, minY = p.height, maxY = 0;
  for (let y = 0; y < p.height; y++) {
    for (let x = 0; x < p.width; x++) {
      const a = p.data[(y * p.width + x) * 4 + 3];
      if (a > 10) {
        if (x < minX) minX = x;
        if (x > maxX) maxX = x;
        if (y < minY) minY = y;
        if (y > maxY) maxY = y;
      }
    }
  }
  console.log(`${rel}: Size=${p.width}x${p.height}, BBox=[${minX},${minY} - ${maxX},${maxY}], CharDim=${maxX - minX + 1}x${maxY - minY + 1}`);
}

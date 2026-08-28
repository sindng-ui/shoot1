const fs = require('fs');
const { PNG } = require('pngjs');

// Measure hand positions on wizard_front, wizard_side, wizard_front_diagonal
const files = [
  'wizard_front.png',
  'wizard_side.png',
  'wizard_front_diagonal.png'
];

for (const f of files) {
  const p = `/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/${f}`;
  const data = PNG.sync.read(fs.readFileSync(p));
  const w = data.width, h = data.height;

  // Dark gloved hand color is dark purple/grey: RGB around [40..70, 30..55, 60..90]
  // Let's find bounding box of hand in lower-middle area (Y: 250..370)
  console.log(`\nAnalyzing ${f} (${w}x${h}):`);
  
  // Center is at X = 175, feet bottom is at Y = 430
  // Hand is roughly Y: 310..370
  let minX = w, maxX = 0, minY = h, maxY = 0;
  for (let y = 300; y < 380; y++) {
    for (let x = 0; x < w; x++) {
      const idx = (y * w + x) << 2;
      const r = data.data[idx], g = data.data[idx+1], b = data.data[idx+2], a = data.data[idx+3];
      if (a > 200) {
        // Glove color: dark grey/violet (r: 30..75, g: 25..60, b: 50..95, lum < 70)
        // Gold trim is yellow (r > 150, g > 120)
        // Robe is purple (r: 60..130, g: 20..70, b: 120..200)
        if (r >= 35 && r <= 85 && g >= 25 && g <= 70 && b >= 50 && b <= 100 && Math.abs(r - g) < 25) {
          if (x < minX) minX = x;
          if (x > maxX) maxX = x;
          if (y < minY) minY = y;
          if (y > maxY) maxY = y;
        }
      }
    }
  }
  console.log(`  Glove area: X=[${minX}..${maxX}], Y=[${minY}..${maxY}]`);
  console.log(`  Canvas center X=175. Offset from center: X=[${minX - 175}..${maxX - 175}]`);
  console.log(`  Feet bottom Y=430. Offset from feet: Y=[${430 - maxY}..${430 - minY}]`);
}

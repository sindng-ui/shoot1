const fs = require('fs');
const { PNG } = require('pngjs');

const p = '/mnt/k/unityprojects/shoot1/shoot1/Assets/Resources/Characters/Wizard/wizard_front_diagonal.png';
const data = PNG.sync.read(fs.readFileSync(p));
const w = data.width, h = data.height;

console.log(`Analyzing wizard_front_diagonal.png:`);
// Let's find the hands!
// Looking at the front diagonal wizard:
// He faces towards bottom-right.
// Left hand (screen-left hand) is at the front or back?
// Right hand (screen-right hand) is at the back or front?
for (let y = 300; y < 400; y += 10) {
  let row = "";
  for (let x = 80; x < 320; x += 10) {
    const idx = (y * w + x) << 2;
    const a = data.data[idx + 3];
    const r = data.data[idx], g = data.data[idx+1], b = data.data[idx+2];
    if (a > 100) {
      if (r < 70 && g < 70 && b < 100 && Math.abs(r-g) < 20) row += "H"; // Glove hand
      else if (r > 150 && g > 120) row += "G"; // Gold
      else row += ".";
    } else {
      row += " ";
    }
  }
  console.log(`Y=${y}: ${row}`);
}

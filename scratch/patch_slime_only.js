const fs = require('fs');

const shPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/SpriteHelper.cs';
const lines = fs.readFileSync(shPath, 'utf8').split('\n');

// Find lines for GetOrCreateSlimeSprite
let startIdx = -1;
let endIdx = -1;

for (let i = 0; i < lines.length; i++) {
  if (lines[i].includes('public static Sprite GetOrCreateSlimeSprite')) {
    startIdx = i;
    // find matching closing brace
    let braceCount = 0;
    for (let j = i; j < lines.length; j++) {
      if (lines[j].includes('{')) braceCount++;
      if (lines[j].includes('}')) {
        braceCount--;
        if (braceCount === 0) {
          endIdx = j;
          break;
        }
      }
    }
    break;
  }
}

if (startIdx !== -1 && endIdx !== -1) {
  console.log(`Replacing lines ${startIdx + 1} to ${endIdx + 1}`);
  lines.splice(startIdx, endIdx - startIdx + 1, '        public static Sprite GetOrCreateSlimeSprite(int size = 28) => MonsterSpriteHelper.GetOrCreateSlimeSprite(size);');
  fs.writeFileSync(shPath, lines.join('\n'), 'utf8');
  console.log('Successfully replaced GetOrCreateSlimeSprite with 1-line forwarding');
} else {
  console.log('Failed to find GetOrCreateSlimeSprite indices:', startIdx, endIdx);
}

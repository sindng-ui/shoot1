const fs = require('fs');

// 1. Update CustomHeroSpriteLoader.cs (increase character size by lowering PPU)
const heroPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/CustomHeroSpriteLoader.cs';
let heroCode = fs.readFileSync(heroPath, 'utf8');

heroCode = heroCode.replace('private const float WarriorPPU = 520f;', 'private const float WarriorPPU = 420f;');
heroCode = heroCode.replace('private const float RangerPPU = 400f;', 'private const float RangerPPU = 320f;');
heroCode = heroCode.replace('private const float WizardPPU = 450f;', 'private const float WizardPPU = 360f;');

fs.writeFileSync(heroPath, heroCode, 'utf8');
console.log('Updated CustomHeroSpriteLoader.cs (Enlarged heroes by ~25%)');

// 2. Update CustomMonsterSpriteLoader.cs (decrease slime size by raising PPU to 1400f)
const monsterPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/CustomMonsterSpriteLoader.cs';
let monsterCode = fs.readFileSync(monsterPath, 'utf8');

monsterCode = monsterCode.replace('private const float SlimePPU = 720f;', 'private const float SlimePPU = 1400f;');

fs.writeFileSync(monsterPath, monsterCode, 'utf8');
console.log('Updated CustomMonsterSpriteLoader.cs (Reduced slime size to ~51%)');

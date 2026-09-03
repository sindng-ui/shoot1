const fs = require('fs');

const path = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/MonsterSpriteHelper.cs';
let code = fs.readFileSync(path, 'utf8');

// Replace checks
code = code.replace(
  'if (_slimeSprite != null) return _slimeSprite;',
  'if (_slimeSprite != null) return _slimeSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Slime) is { } cs) return _slimeSprite = cs;'
);

code = code.replace(
  'if (_batSprite != null) return _batSprite;',
  'if (_batSprite != null) return _batSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Bat) is { } cb) return _batSprite = cb;'
);

code = code.replace(
  'if (_skeletonSprite != null) return _skeletonSprite;',
  'if (_skeletonSprite != null) return _skeletonSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Skeleton) is { } csk) return _skeletonSprite = csk;'
);

code = code.replace(
  'if (_golemSprite != null) return _golemSprite;',
  'if (_golemSprite != null) return _golemSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Golem) is { } cg) return _golemSprite = cg;'
);

code = code.replace(
  'if (_fireImpSprite != null) return _fireImpSprite;',
  'if (_fireImpSprite != null) return _fireImpSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.FireImp) is { } cf) return _fireImpSprite = cf;'
);

code = code.replace(
  'if (_toxicSpiderSprite != null) return _toxicSpiderSprite;',
  'if (_toxicSpiderSprite != null) return _toxicSpiderSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.ToxicSpider) is { } ct) return _toxicSpiderSprite = ct;'
);

code = code.replace(
  'if (_darkKnightSprite != null) return _darkKnightSprite;',
  'if (_darkKnightSprite != null) return _darkKnightSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.DarkKnight) is { } cd) return _darkKnightSprite = cd;'
);

fs.writeFileSync(path, code, 'utf8');
console.log('Updated MonsterSpriteHelper.cs');

// Phase3MonsterSpriteHelper.cs
const p3Path = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/Phase3MonsterSpriteHelper.cs';
let p3Code = fs.readFileSync(p3Path, 'utf8');

p3Code = p3Code.replace(
  'if (_cachedLichKingSprite != null) return _cachedLichKingSprite;',
  'if (_cachedLichKingSprite != null) return _cachedLichKingSprite;\n            if (CustomMonsterSpriteLoader.TryGetCustomMonsterSprite(HappyShoot.Domain.Entities.MonsterType.Boss3) is { } cl) return _cachedLichKingSprite = cl;'
);

fs.writeFileSync(p3Path, p3Code, 'utf8');
console.log('Updated Phase3MonsterSpriteHelper.cs');

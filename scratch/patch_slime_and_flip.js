const fs = require('fs');

// 1. Patch SpriteHelper.cs
const shPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/SpriteHelper.cs';
let shCode = fs.readFileSync(shPath, 'utf8');

// Replace the old GetOrCreateSlimeSprite block with forwarding
const oldSlimeStart = '/// <summary>\n        /// 24x24 Bouncy Jelly Slime.\n        /// </summary>\n        public static Sprite GetOrCreateSlimeSprite(int size = 24)';
// Let's find where GetOrCreateSlimeSprite begins and ends
const slimeIdx = shCode.indexOf('public static Sprite GetOrCreateSlimeSprite');
if (slimeIdx !== -1) {
  // Find start of method (summary)
  const commentStart = shCode.lastIndexOf('/// <summary>', slimeIdx);
  // Find next method or comment
  const nextMethod = shCode.indexOf('/// <summary>', slimeIdx);
  
  // Replace the old method implementation with single-line forwarding
  const oldMethodRegex = /\/\/\/\s*<summary>[\s\S]*?24x24 Bouncy Jelly Slime[\s\S]*?return _slimeSprite;\s*\}/;
  if (oldMethodRegex.test(shCode)) {
    shCode = shCode.replace(oldMethodRegex, 'public static Sprite GetOrCreateSlimeSprite(int size = 28) => MonsterSpriteHelper.GetOrCreateSlimeSprite(size);');
    fs.writeFileSync(shPath, shCode, 'utf8');
    console.log('Successfully patched SpriteHelper.cs GetOrCreateSlimeSprite forwarding');
  } else {
    console.log('Regex match failed for old GetOrCreateSlimeSprite');
  }
}

// 2. Patch MonsterView.cs for flipX
const mvPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterView.cs';
let mvCode = fs.readFileSync(mvPath, 'utf8');

// Add _lastX field
if (!mvCode.includes('private float _lastX;')) {
  mvCode = mvCode.replace(
    'private float _baseScale = 1.4f;',
    'private float _baseScale = 1.4f;\n        private float _lastX;'
  );
}

// In Bind method, initialize _lastX
if (!mvCode.includes('_lastX = entity.Position.X;')) {
  mvCode = mvCode.replace(
    '_hurtJoltTimer = 0f;',
    '_hurtJoltTimer = 0f;\n            _lastX = entity.Position.X;\n            if (_spriteRenderer != null) _spriteRenderer.flipX = false;'
  );
}

// In UpdateView method, add flipX logic
if (!mvCode.includes('deltaX > 0.005f')) {
  mvCode = mvCode.replace(
    '_transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);',
    `_transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);

            // Handle horizontal direction flip (moving right -> flipX true, moving left -> flipX false)
            float deltaX = _entity.Position.X - _lastX;
            _lastX = _entity.Position.X;
            if (deltaX > 0.005f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = true;
            }
            else if (deltaX < -0.005f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = false;
            }`
  );
}

fs.writeFileSync(mvPath, mvCode, 'utf8');
console.log('Successfully patched MonsterView.cs flipX logic');

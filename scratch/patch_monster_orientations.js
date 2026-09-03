const fs = require('fs');

const mvPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterView.cs';
let code = fs.readFileSync(mvPath, 'utf8');

// Replace the flip logic in UpdateView
const oldFlipBlock = `            // Handle horizontal direction flip (moving right -> flipX true, moving left -> flipX false)
            float deltaX = _entity.Position.X - _lastX;
            _lastX = _entity.Position.X;
            if (deltaX > 0.005f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = true;
            }
            else if (deltaX < -0.005f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = false;
            }`;

const newFlipBlock = `            // Handle horizontal direction flip based on each monster's native art orientation
            float deltaX = _entity.Position.X - _lastX;
            _lastX = _entity.Position.X;
            bool origRight = IsOriginalArtFacingRight();

            if (deltaX > 0.005f)
            {
                // Moving Right: face right
                if (_spriteRenderer != null) _spriteRenderer.flipX = !origRight;
            }
            else if (deltaX < -0.005f)
            {
                // Moving Left: face left
                if (_spriteRenderer != null) _spriteRenderer.flipX = origRight;
            }`;

code = code.replace(oldFlipBlock, newFlipBlock);

// Add IsOriginalArtFacingRight helper method if not present
if (!code.includes('IsOriginalArtFacingRight()')) {
  const helperMethod = `
        /// <summary>
        /// Returns true if the native monster sprite art is facing Right.
        /// Right: Skeleton, Golem, FireImp.
        /// Left: DarkKnight, LichKing, ToxicSpider, VampireBat, Slime.
        /// </summary>
        private bool IsOriginalArtFacingRight()
        {
            if (_entity == null) return false;
            return _entity.Type == MonsterType.Skeleton
                || _entity.Type == MonsterType.Golem
                || _entity.Type == MonsterType.FireImp;
        }
`;

  // Insert before UpdateStatusTint or private bool _isCritHit
  code = code.replace('private bool _isCritHit = false;', helperMethod + '        private bool _isCritHit = false;');
}

fs.writeFileSync(mvPath, code, 'utf8');
console.log('Successfully updated MonsterView.cs with per-monster native orientation flipX logic');

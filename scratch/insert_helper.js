const fs = require('fs');

const mvPath = '/mnt/k/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterView.cs';
let code = fs.readFileSync(mvPath, 'utf8');

const helperMethod = `        /// <summary>
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

        private bool _isCritHit = false;`;

if (!code.includes('private bool IsOriginalArtFacingRight()')) {
  code = code.replace('private bool _isCritHit = false;', helperMethod);
  fs.writeFileSync(mvPath, code, 'utf8');
  console.log('Inserted IsOriginalArtFacingRight() method definition');
}

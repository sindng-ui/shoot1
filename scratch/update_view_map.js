const fs = require('fs');

const path = '/mnt/k/unityprojects/shoot1/shoot1/docs/app_map/VIEW_MAP.md';
let content = fs.readFileSync(path, 'utf8');

const target = '| | `MonsterSpriteHelper.cs` | `MonsterSpriteHelper` | 7종 일반 몬스터 + 보스 2종 고해상도 셀 셰이딩/글레어/발광 코어/룬 픽셀아트 생성기 |';
const replacement = '| | `MonsterSpriteHelper.cs` | `MonsterSpriteHelper` | 7종 일반 몬스터 + 보스 2종 고해상도 셀 셰이딩/글레어/발광 코어/룬 픽셀아트 생성기 |\n| | `CustomMonsterSpriteLoader.cs` | `CustomMonsterSpriteLoader` | 8종 몬스터(슬라임/박쥐/스켈레톤/임프/거미/다크나이트/골렘/리치왕) 고해상도 커스텀 스프라이트 4단계 안전 로더 (타입별 최적 PPU 600~750f, 피벗 자동 보정, FilterMode.Point, 100% 절차적 Fallback) |';

if (content.includes(target)) {
  content = content.replace(target, replacement);
  fs.writeFileSync(path, content, 'utf8');
  console.log('Successfully updated VIEW_MAP.md');
} else {
  console.log('Target string not found in VIEW_MAP.md');
}

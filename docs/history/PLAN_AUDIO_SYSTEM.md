# 🎵 Phase 5: 사운드 매니저 & 프로시저럴 오디오 풀링 시스템 구현 계획서

## 1. 개요
- **목표**: Pure C# Domain의 이벤트 버스(`EventBus`)와 완벽히 디커플링된 고성능 오디오 시스템을 구축하고, 외부 오디오 파일 없이도 Unity `AudioClip.Create` + 수학적 파형 합성을 통해 14가지 SFX 및 아케이드 칩튠 BGM을 무할당 프로시저럴 생성/재생하는 시스템 구축.
- **아키텍처**:
  - Domain: `AudioEvents.cs` (`SoundEffectType`, `PlaySoundEvent`, `PlayBgmEvent`)
  - View: `ProceduralAudioHelper.cs` (수학적 파형 합성기), `SoundManagerView.cs` (16채널 오디오 소스 풀링, 디바운싱, BGM 루프)
  - Tests: `AudioEventsTests.cs` (도메인 오디오 이벤트 발행/수신 100% 검증)

---

## 2. 세부 구현 내역

### 1) 14종 프로시저럴 SFX & 칩튠 BGM 합성기 (`ProceduralAudioHelper.cs`)
- **무기/스킬 효과음**:
  - `SlashAttack`: 날카로운 스위프 노이즈 + 고주파 톱니파
  - `BowShoot`: 짧은 피치 드롭 핑 사인파
  - `MagicExplosion`: 저주파 럼블 노이즈 + 감쇠 파형
  - `BladeOrbit`: 주기적 금속 마찰 공명음
- **타격 & 피격 & 처치**:
  - `MonsterHit`: 찰진 둔탁한 타격음
  - `MonsterDeath`: 하강 피치 레트로 디졸브 사운드
  - `PlayerHurt`: 강렬한 경고성 저음 타격
- **성장 & 보상 & 보스**:
  - `GemCollect`: 경쾌한 고주파 크리스탈 딩음
  - `LevelUp`: 상승 4화음 팡파레
  - `WeaponEvolve`: 장엄한 8비트 아르페지오 신스 팡파레
  - `BossSpawn`: 불길한 저음 사이렌/경보음
  - `ChestOpen`: 반짝이는 황금 보상 오픈 징글
  - `GameOver`: 하강 슬픈 징글
  - `Victory`: 승리 트라이엄프 팡파레
- **BGM**:
  - 8비트 레트로 아케이드 칩튠 베이스/아르페지오 8초 루프 사운드트랙

### 2) 고성능 16채널 오디오 풀링 & 디바운서 (`SoundManagerView.cs`)
- **Zero-GC AudioSource Pooling**: 16개의 AudioSource를 사전 생성하여 동시 타격 시 가용 소스를 회전 할당
- **Sound Debouncing**: 초당 수십 회 발생하는 몬스터 피격(`MonsterHit`) 등의 효과음에 0.05s 쿨다운을 부여하여 사운드 뭉개짐(Distortion) 및 오디오 오버헤드 완벽 방지
- **BGM 제어**: 게임 상태(Playing $\rightarrow$ Paused $\rightarrow$ GameOver)에 따라 볼륨/피치 부드러운 전환

### 3) 단위 테스트 계획 (`AudioEventsTests.cs`)
- 도메인 이벤트 버스를 통한 사운드 이벤트 발행, 다중 구독자 수신, 이벤트 타입별 올바른 페이로드 전달 검증

---

## 3. 검증 계획
- Roslyn / Mono CLI 테스트 스위트 실행 (총 85+개 단위 테스트 전원 통과 목표)
- 모든 파일 500줄 이하 엄격 유지
- `APP_MAP.md` 및 `docs/TEST_RESULTS_PHASE5.txt` 업데이트

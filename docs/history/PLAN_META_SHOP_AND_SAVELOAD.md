# 🏛️ Phase 4: 메타 성장 영구 상점 UI & 로컬 세이브/로드 시스템 구현 계획서

## 1. 개요
- **목표**: 인게임에서 획득한 골드를 로컬 영구 세이브(`JsonPlayerPrefsStorage`)에 자동 정산 누적하고, 메타 상점 UI(`MetaShopUiView`)를 통해 8가지 영구 스탯을 강화/100% 무료 환불하며, 게임 시작 시 캐릭터 스탯에 영구 강화를 자동 반영하는 완전한 메타 루프 완성.
- **아키텍처**: Pure C# Domain 레이어의 `MetaShopManager`, `MetaUpgradeApplier`, `MetaUpgradeSaveData`와 Unity Presentation 레이어의 `MetaShopUiView`, `GameOverResultUiView`, `GameBootstrap` 간 완벽 연동.

---

## 2. 세부 구현 내역

### 1) 영구 강화 8종 카테고리
1. **생존 (Survival)**:
   - Max Health (+10 HP/Lv, Max 10Lv)
   - Armor (+2 Armor/Lv, Max 5Lv)
   - Recovery (+0.2 HP/s/Lv, Max 5Lv)
2. **공격 (Offense)**:
   - Might (+5% Damage/Lv, Max 10Lv)
   - Critical (+2% Crit Chance/Lv, Max 5Lv)
   - Amount (+1 Extra Projectile/Lv, Max 2Lv)
3. **유틸리티 (Utility)**:
   - Haste (+5% Move Speed/Lv, Max 5Lv)
   - Magnet (+0.5m Pickup Radius/Lv, Max 5Lv)

### 2) 상점 UI & 100% 무료 환불 시스템
- `MetaShopUiView.cs`:
  - 8종 업그레이드 항목별 이름, 설명, 현재 레벨/최대 레벨, 강화 비용, [UPGRADE] 버튼 생성
  - 상단 보유 골드 실시간 갱신 (`Gold: 1,250`)
  - [REFUND ALL] 버튼: 투자된 모든 골드를 100% 전액 환불하고 스탯 레벨 0으로 초기화
  - [CLOSE / BACK] 버튼: 상점 닫기

### 3) 게임오버 정산 & 스탯 적용 루프
- `GameOverResultUiView`: 게임 오버 시 획득 골드를 `MetaShopManager.AddGold()`로 자동 영구 저장 & [OPEN SHOP] 버튼 제공
- `GameBootstrap`: 영구 저장된 업그레이드 데이터를 불러와 `MetaUpgradeApplier.ApplyUpgrades(baseStats, saveData)`로 캐릭터 스탯에 적용

---

## 3. 단위 테스트 계획
- `MetaShopTests.cs`: 골드 추가, 업그레이드 레벨별 구매/골드 차감, 최대 레벨 초과 구매 방지, 100% 환불 골드 계산 검증
- `MetaSaveDataTests.cs`: 업그레이드 데이터 직렬화 및 `MetaUpgradeApplier` 스탯 반영 공식 검증

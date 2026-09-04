# 💥 고선명 그래픽 데미지 텍스트 및 속성별 비주얼/성능 최적화 구현 계획서

> **작성일자**: 2026-09-05  
> **목표**: 캡처된 레퍼런스 게임처럼 밝은 배경 및 이펙트 속에서도 칼같이 돋보이는 굵은 검은색 테두리(Heavy Black Outline) 그래픽 볼드 폰트 적용, 속성별(일반-흰색, 불-화염주황, 얼음-빙결하늘색, 전기-네온노랑) 전용 컬러링, 크리티컬 대각선 틸트 연출, 그리고 모바일 환경 0-GC 무할당 성능 보장.

---

## 1. 🎯 사용자 요구사항 분석 및 해결 전략

| 번호 | 요구사항 | 현 상태 분석 | 해결 방안 |
| :--- | :--- | :--- | :--- |
| **1** | **크고 잘 보이는 예쁜 그래픽 폰트** (모바일 고려) | 얇은 기본 시스템 고딕(Malgun Gothic/Arial), 외곽선 없음 | • 게이밍 볼드 서체(Impact, Arial Black, Malgun Gothic Bold 등) 우선 탐색 로더 도입<br>• **2~3px 고선명 블랙 아웃라인 셰이더 (`DamageTextOutline.shader`)** 적용<br>• 기본 크기 및 스케일 대폭 상향 (모바일 시인성 극대화)<br>• 천 단위 쉼표 포맷팅 (`1,428`) |
| **2** | **일반 데미지 = 흰색** | 현재 일반 흰색이나 외곽선 없어 사막/폭발 배경에 묻힘 | • 본체 색상: **완전한 순백색 (`#FFFFFF`)**<br>• 짙은 검은색 테두리와 대비되어 밝은 모래/폭발 속에서도 선명하게 시인 |
| **3** | **전기 / 얼음 / 불 각 데미지 색상 차별화** | `DamageType`이 데미지 텍스트로 전달되지 않음 | • **불(Fire/Burn)**: 타오르는 화염 주황 (`#FF6B00` ~ `#FF7A1A`)<br>• **얼음(Ice/Frost)**: 차갑고 맑은 빙결 시안 (`#00E5FF` ~ `#38D9FF`)<br>• **전기(Lightning/Shock)**: 찌릿찌릿한 네온 옐로우 (`#FFE600` ~ `#FFF01F`)<br>• 서리폭발/연쇄번개 등 스킬별 `DamageType` 도메인 이벤트 완전 연동 |
| **4** | **크리티컬 데미지: 더 크고 대각선 표시** | 크기만 1.35x 증가, 회전 없음 | • 폰트 크기 및 스케일 1.45배 대폭 확대<br>• **살짝 대각선 틸트 (`z = -12도` 사선 각도)** 적용<br>• 스폰 시점 1회 회전 설정으로 **런타임 연산 부하 0.000% 달성**<br>• 역동적인 펀치 바운스 팝업 및 느낌표(`!`) 표기 |
| **5** | **성능 저하 제로 (Zero Performance Degradation)** | `ToString("0")` 문자열 힙 할당 발생, 풀 32개 고정 | • **무할당 정수 문자열 캐시 (`DamageNumberCache`)**: 0~3000 데미지 0-Alloc 메모이제이션<br>• **1-Pass 언릿 아웃라인 셰이더**: 버텍스 버퍼 추가/오브젝트 증가 0, 드로우콜 1개 유지<br>• 풀 64개 확장 및 LRU 안전 재활용으로 GC 프리징 및 프레임 드랍 원천 차단 |

---

## 2. 🏗️ 파일 분리 및 모듈 아키텍처 (500줄 준수)

단일 파일 비대화를 방지하고 클린 아키텍처를 유지하기 위해 기능별로 명확히 분리합니다:

1. **`Assets/src/HappyShoot.View/UI/DamageFontHelper.cs`** (신규)
   - Impact, Arial Black, Segoe UI Black, Roboto-Black, Malgun Gothic Bold 등 게이밍 볼드 폰트 동적 로드 및 캐싱.
2. **`Assets/src/HappyShoot.View/UI/DamageNumberCache.cs`** (신규)
   - 0 ~ 3000 범위의 일반 및 크리티컬 숫자 문자열(쉼표 및 느낌표 포함)을 0-Alloc으로 제공하는 정적 캐시.
3. **`Assets/src/HappyShoot.View/UI/DamageColorPalette.cs`** (신규)
   - 일반(흰색), 불(주황), 얼음(시안), 전기(네온노랑), 크리티컬(골드하이라이트) 색상 상수 및 헬퍼 정의.
4. **`Assets/Resources/Shaders/DamageTextOutline.shader`** (신규)
   - 폰트 텍스처를 8방향 팽창 샘플링하여 짙은 검은색 테두리를 1패스로 그리는 초경량 고성능 셰이더.
5. **`Assets/src/HappyShoot.View/UI/DamageTextView.cs`** (수정)
   - 새 셰이더 및 머티리얼 바인딩, 속성별 색상/크기/대각선 회전 적용.
6. **`Assets/src/HappyShoot.View/UI/DamageTextManagerView.cs`** (수정)
   - 풀 64개 확장 및 최적화된 스폰/업데이트.
7. **`Assets/src/HappyShoot.Domain/Events/MonsterEvents.cs` & `DamageTextEntity.cs` / `DamageTextManager.cs`** (수정)
   - `DamageType.Ice`, `DamageType.Lightning` 열거형 추가 및 도메인 엔티티 속성 전달 연동.
8. **`FrostNovaEffect.cs`, `BlizzardNovaEffect.cs`, `ChainLightningEffect.cs`** (수정)
   - 스킬별 데미지 타입 정확히 전달.

---

## 3. 🧪 검증 계획

1. **도메인 단위 테스트**:
   - `DamageTextTests.cs`: `DamageType` 속성이 엔티티에 정확히 전달되고 유지되는지 테스트 추가.
2. **비주얼 & 성능 검증**:
   - 일반 피격: 흰색 본체 + 굵은 검은색 외곽선 확인.
   - 불꽃 화염구/화상: 주황색 데미지 텍스트 확인.
   - 서리 폭발/블리자드: 얼음 하늘색 데미지 텍스트 확인.
   - 연쇄 번개/감전: 네온 노란색 데미지 텍스트 확인.
   - 크리티컬: 1.45배 대형 크기 + -12도 대각선 틸트 연출 확인.
   - 모바일 프로파일링: 초당 수십 개 스폰 시 GC Alloc 발생 여부 및 드로우콜 체크.

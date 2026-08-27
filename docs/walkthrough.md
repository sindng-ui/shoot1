# 🏛️ 무한 배경 타일링 시스템 및 고대 던전 전장 비주얼 구현 결과

## 1. 개요
플레이어가 광활한 전장을 이동할 때 카메라 영역에 맞춰 배경 타일이 끊김 없이 자연스럽게 순환 이동(Wrap-around)하는 무한 배경 타일링 시스템을 구축하였습니다.
기존의 밋밋한 단색 카메라 배경에서 벗어나, 고대 석판 바닥 질감, 금 간 석판, 고대 마법 룬 문양, 미세한 이끼 디테일, 그리고 전장에 깊이감을 부여하는 앰비언트 부유 먼지 입자까지 갖춘 **"진짜 서바이버즈 게임다운 고품질 전장"**을 완성하였습니다.

---

## 2. 신규 모듈 구성 및 라인 수 (500줄 규칙 100% 준수)
- [BackgroundTileView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Background/BackgroundTileView.cs): 56줄
- [BackgroundAmbientDustView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Background/BackgroundAmbientDustView.cs): 125줄
- [BackgroundManager.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Background/BackgroundManager.cs): 156줄
- [BackgroundSpriteHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Background/BackgroundSpriteHelper.cs): 277줄
- [GameBootstrap.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs): 365줄

---

## 3. 주요 핵심 기능
1. **3x3 무한 랩어라운드 타일링**:
   - 24m x 24m 타일 9장(72m x 72m 커버).
   - 21:9 울트라와이드 모니터 및 메테오/지각변동 쉐이크 시에도 화면 끝이 비지 않는 여유 마진.
   - 매 프레임 GC Alloc = 0 Bytes, float 오프셋 기반 무할당 순환 이동.
2. **4종 고대 석판 바리에이션 프로시저럴 픽셀아트**:
   - Classic(기본), Cracked(균열), Runic(마법룬), Moss(이끼) 4종 타일 자동 스왑 배치.
3. **28개 앰비언트 부유 먼지 입자 (Ambient Floating Motes)**:
   - 전장에 공기감과 깊이감을 제공하며 화면 이탈 시 무할당 순환.
4. **2.5D 캐릭터/몬스터 그림자 시인성 대폭 강화**:
   - 기존 32x16의 옅은 가우시안 알파(45%)에서 **48x24 고해상도 + 85% 불투명도 코어**의 선명한 카툰 타원 블롭 섀도우로 개편.
   - 배경 석판 타일의 명암비를 자연스러운 중세 슬레이트 톤(`RGB 0.22~0.25`)으로 조화롭게 밸런싱하여, 어두운 바닥 위에서도 발밑 그림자가 깔끔하고 선명하게 도드라지도록 완성.
5. **소팅 오더 무결성**:
   - 배경 타일: `-100`, 앰비언트 입자: `-50`, 그림자: `-10`, 몬스터: `10`, 플레이어: `15`, 투사체: `20~30` 분리로 완벽한 렌더링 순서 보장.

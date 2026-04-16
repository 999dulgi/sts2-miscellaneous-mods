# 새 모드 CLAUDE.md 초안 — 기본 경로/위치 정보

---

## 모드 개요

**목적**: 전투 중 발생한 이벤트(카드 플레이, 적 행동 등)를 하스스톤의 좌측 히스토리 패널처럼 화면에 순서대로 표시하고, 항목을 클릭하면 해당 턴으로 되돌아갈 수 있는 턴 되감기/히스토리 모드.

**핵심 기능**:

1. **이벤트 히스토리 패널** — 매 턴 발생한 일(카드 사용, 피해, 버프 등)을 리스트로 표시
2. **턴 점프** — 히스토리 항목 클릭 시 해당 턴의 게임 상태로 복원

---

## 개발 환경 경로

- **게임 설치 경로**: `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\`
- **게임 실행 파일**: `...\SlayTheSpire2.exe`
- **게임 어셈블리**: `...\SlayTheSpire2_Data\Managed\` (Unity) 또는 게임 빌드 방식에 따라 다름
- **Godot 프로젝트 루트**: `C:\Users\rlawh\Documents\<new-mod-name>\`
- **디컴파일 소스**: `C:\Users\rlawh\sts2Decompile` — `sts2.dll`을 디컴파일한 게임 소스 파일들의 디렉토리. 패치 대상 클래스/메서드 탐색 시 참조.

---

## 모드 관련 경로

- **모드 설치 폴더**: `%APPDATA%\SlaytheSpire2\mods\<mod-id>\`
- **모드 설정 파일**: `%APPDATA%\SlaytheSpire2\<mod-id>.config.json`
- **모드 로그**: `%APPDATA%\SlaytheSpire2\logs\` (게임 로그 위치, 확인 필요)
- **빌드 출력 (DLL)**: `<project-root>\bin\Debug\net9.0\` 또는 export 경로

---

## Godot + C# 프로젝트 구조 (기본 템플릿)

```text
<project-root>/
  <ModName>.csproj
  <ModName>.sln
  <ModName>_manifest.json      # 모드 메타데이터
  project.godot
  MainPatch.cs                 # Harmony 패치 진입점
  ...
```

---

## manifest.json 기본 형식

```json
{
  "id": "<mod-id>",
  "name": "<모드 이름>",
  "author": "999dulgi",
  "description": "<모드 설명>",
  "version": "1.0.0",
  "has_pck": pck여부,
  "has_dll": true,
  "affects_gameplay": false
}
```

---

## Harmony 패치 기본 진입점

```csharp
[HarmonyPatch(typeof(대상클래스), nameof(대상클래스.대상메서드))]
public class 
{
    {
        // 패치 내용
    }
}
```

---

## 빌드 / 배포

- **빌드**: Godot 에디터에서 export 또는 `dotnet publish sts2-history.csproj`
- **DLL 복사 위치**: `%APPDATA%\SlaytheSpire2\mods\<mod-id>\<ModName>.dll`
- **참조 어셈블리 경로**: 게임 설치 경로 내 DLL (HarmonyLib.dll, 게임 어셈블리 등)

---

## AI 작업 지침

- thinking 과정에서 `wait`의 사용을 최소화하고, 최대 3번까지만 허용한다.

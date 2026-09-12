# OIL BREAK 유니티 설정창 UI

Unity 2021.3 LTS의 기본 uGUI 기준입니다.

## 설치

1. 압축을 풉니다.
2. 안의 `Assets/OilBreakSettingsUI` 폴더를 Unity 프로젝트의 `Assets` 폴더 안으로 복사합니다.
3. Unity가 컴파일을 마칠 때까지 기다립니다.
4. 상단 메뉴에서 `Tools > OIL BREAK > Create Settings UI`를 누릅니다.
5. 현재 Scene에 `OilBreakSettingsCanvas`가 자동 생성됩니다.

## 시작 화면의 설정 버튼 연결

1. 시작 화면의 설정 버튼을 선택합니다.
2. Button 컴포넌트의 `On Click()`에서 `+`를 누릅니다.
3. 생성된 `SettingsPanel` 오브젝트를 칸에 드래그합니다.
4. 함수에서 `GameObject > SetActive(bool)`을 선택하고 체크합니다.

## Audio Mixer 연결(선택)

Audio Mixer를 사용한다면 `SettingsPanel`의 `OilBreakSettingsController`에 Mixer를 넣고,
노출 파라미터 이름을 `MasterVolume`, `BGMVolume`, `SFXVolume`으로 맞춥니다.
Mixer를 연결하지 않아도 전체 음량, 마우스 감도, 전체 화면, 설정 저장은 작동합니다.

## 마우스 감도 적용

플레이어 카메라 회전 코드에서 기존 감도 대신 아래 값을 곱합니다.

```csharp
float sensitivity = OilBreak.UI.OilBreakSettingsController.MouseSensitivity;
```

설정값은 PlayerPrefs에 저장되어 게임을 껐다 켜도 유지됩니다.

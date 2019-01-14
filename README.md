
연결 프로세스의 화면을 분석해서 키보드나 마우스 매크로 설정하여 키보드나 마우스 이벤트 반복

<img src="https://github.com/EomTaeWook/EmulatorMacro/blob/master/Release/Resource/capture.png" width="100%"></img>

개발환경

    WPF, C#, .net 4.6.1
    
OS 버전

    Window 8.0 이상 Window 7에서는 작동 안하는 기능이 있음.

사용 방법

    1.화면 캡쳐 버튼를 통하여 이미지 캡쳐
    
    2.마우스 키보드 이벤트 선택
    
        2.1마우스 이벤트인 경우 좌표 지정 해주시면 됩니다.
        
        2.2키보드 이벤트인 경우 Ctrl + c + v 이런식으로 조합키를 넣어주면 됩니다.
        
    3.캡쳐할 실행 프로세스를 선택하시면 됩니다.
    
설정(Config.json)

    1.Language 언어 : [Eng],[Kor]
    
    2.SavePath : 설정 리스트 save 경로
    
    3.Period : 전체 작업 완료 이후 딜레이

    4.ProcessDelay : 저장된 트리거 아이템 비교 완료 이후 딜레이
    
    5.Similarity : 이미지 프로세싱 유사도

작업 목록

   2019-01-07 Feedback

    8.이벤트 트리거 설정 저장 방식 변경
    
        8.2 서브 이벤트 트리거 설정

        8.3 에뮬레이터 사이즈 변경시 크기 비율에 따른 캡쳐 이미지 사이즈 변경

    11.OpenCV ROI 기능 적용

    13.ListView TreeView 변경 하위 이벤트 트리거 볼 수 있도록 UI 변경

    14.Mouse trigger 좌표 값 Test

        14.1 모니터간 DPI 값이 다를 경우 에뮬레이터 이동시 좌표값 틀려짐 보정 필요


릴리즈

1.3.0

    리스트 Drag And Drop으로 순서 변경 기능 추가

1.2.2

    버전 체크 및 Install 방식 변경

1.2.1

    Mouse Event 방식 변경

    물리적인 마우스 이벤트 -> 논리적 마우스 이벤트 방식의 변경

    프로그래스바 버그 수정

1.2.0 

    setting view 추가

    2019-01-09 Feedback

    작업간의 중간 딜레이 설정 기능 추가, 딜레이를 통한 CPU 사용률 저하 확인

1.1.1

    UI 비동기 처리가 안 되어서 프로그램이 응답없음 뜨는 오류 수정.

1.1.0

    세이브 파일 확장성 고려 및 에뮬레이터 사이즈 변경없이 이동시키는 경우 마우스 보정 작업

1.0.0 

    기본 기능 완료

버그 레포팅

    enter0917@naver.com

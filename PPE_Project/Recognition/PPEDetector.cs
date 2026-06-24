using OpenCvSharp;

namespace PPE_Project.Recognition
{
    public class PPEDetector
    {
        // 흰색 안전모 HSV 범위
        private Scalar hardHatLower = new Scalar(0, 0, 200);    // H, S, V 최솟값
        private Scalar hardHatUpper = new Scalar(180, 40, 255); // H, S, V 최댓값

        // 형광 조끼 HSV 범위 (노란~초록 형광)
        private Scalar vestLower = new Scalar(30, 100, 100);    // H, S, V 최솟값
        private Scalar vestUpper = new Scalar(90, 255, 255);    // H, S, V 최댓값

        private bool isPPEEnabled = false;   // PPE 감지 활성화 여부
        private bool checkHardHat = true;    // 안전모 감지 여부
        private bool checkVest = true;       // 조끼 감지 여부

        // PPE 감지 활성화/비활성화
        public void SetPPEEnabled(bool enabled) => isPPEEnabled = enabled;

        // 감지 모드 설정 (안전모만 / 조끼만 / 모두)
        public void SetCheckMode(bool hardHat, bool vest)
        {
            checkHardHat = hardHat;
            checkVest = vest;
        }

        public bool TogglePPE()
        {
            isPPEEnabled = !isPPEEnabled;
            return isPPEEnabled;
        }

        // PPE 감지 메인 메서드
        public (bool hardHatDetected, bool vestDetected) Detect(Mat frame)
        {
            if (!isPPEEnabled) return (true, true);  // 비활성화면 둘 다 true 반환

            // BGR → HSV 변환
            Mat hsv = new Mat();
            Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

            bool hardHatDetected = !checkHardHat;  // 안전모 감지 안 하면 기본 true
            bool vestDetected = !checkVest;         // 조끼 감지 안 하면 기본 true

            // 안전모 감지
            if (checkHardHat)
            {
                Mat hardHatMask = new Mat();
                Cv2.InRange(hsv, hardHatLower, hardHatUpper, hardHatMask);  // 흰색 범위 마스크 생성
                int whitePixels = Cv2.CountNonZero(hardHatMask);            // 흰색 픽셀 수 세기
                hardHatDetected = whitePixels > 3000;                        // 3000픽셀 이상이면 착용
            }

            // 조끼 감지
            if (checkVest)
            {
                Mat vestMask = new Mat();
                Cv2.InRange(hsv, vestLower, vestUpper, vestMask);   // 형광색 범위 마스크 생성
                int vestPixels = Cv2.CountNonZero(vestMask);        // 형광색 픽셀 수 세기
                vestDetected = vestPixels > 3000;                    // 3000픽셀 이상이면 착용
            }

            return (hardHatDetected, vestDetected);
        }
    }
}
using OpenCvSharp;
using OpenCvSharp.Face;
using PPE_Project.Database;
using System.Collections.Generic;

namespace PPE_Project.Recognition
{
    public class FaceRecognizer
    {
        private LBPHFaceRecognizer recognizer;       // LBPH 얼굴 인식 모델
        private Dictionary<int, string> employeeMap; // id → 이름 매핑 테이블
        private bool isTrained = false;              // 학습 완료 여부

        public FaceRecognizer()
        {
            recognizer = LBPHFaceRecognizer.Create();  // LBPH 모델 생성
            employeeMap = new Dictionary<int, string>();
        }

        // 모델 학습
        public void Train()
        {
            var faceDataList = DB.GetFaceData();       // DB에서 얼굴 데이터 가져오기

            if (faceDataList.Count == 0) return;       // 등록된 얼굴 없으면 학습 안 함

            var images = new List<Mat>();              // 학습용 이미지 리스트
            var labels = new List<int>();              // 각 이미지에 해당하는 사원 id 리스트

            foreach (var (id, faceData) in faceDataList)
            {
                // byte[] → Mat 변환
                Mat img = Cv2.ImDecode(faceData, ImreadModes.Grayscale);

                if (img.Empty()) continue;

                images.Add(img);
                labels.Add(id);
            }

            if (images.Count == 0) return;

            recognizer.Train(images, labels);
            // id → 이름 매핑 테이블 채우기
            var employees = DB.GetEmployees();
            foreach (var employee in employees)
            {
                employeeMap[employee.Id] = employee.Name;
            }// 모델 학습
            isTrained = true;
        }

        // 얼굴 예측
        public (int employeeId, double confidence) Predict(Mat faceImage)
        {
            if (!isTrained) return (-1, 0);            // 학습 안 됐으면 -1 반환

            // 흑백 변환 (학습할 때랑 같은 형식으로)
            Mat gray = new Mat();
            if (faceImage.Channels() > 1)
            {
                Cv2.CvtColor(faceImage, gray, ColorConversionCodes.BGR2GRAY);
            }
            else
            {
                gray = faceImage;
            }

            recognizer.Predict(gray, out int label, out double confidence);
            return (label, confidence);                // 사원 id랑 신뢰도 반환


        }
        public string GetName(int employeeId)
        {
            if (employeeMap.ContainsKey(employeeId))
                return employeeMap[employeeId];   // id에 해당하는 이름 반환
            return "알 수 없음";
        }
    }
}
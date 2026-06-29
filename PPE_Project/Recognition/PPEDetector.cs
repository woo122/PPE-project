using OpenCvSharp;

namespace PPE_Project.Recognition
{
    public class PPEDetector
    {
        // 노란색 안전모 HSV 범위
        private Scalar hardHatLower = new Scalar(20, 100, 100);   // 노란색 시작
        private Scalar hardHatUpper = new Scalar(35, 255, 255);   // 노란색 끝

        private bool isPPEEnabled = false;

        public bool IsPPEEnabled() => isPPEEnabled;

        public bool TogglePPE()
        {
            isPPEEnabled = !isPPEEnabled;
            return isPPEEnabled;
        }

        public (bool hardHatDetected, Rect hardHatRect) Detect(Mat frame)
        {
            if (!isPPEEnabled) return (true, new Rect());

            Mat hsv = new Mat();
            Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

            Mat hardHatMask = new Mat();
            Cv2.InRange(hsv, hardHatLower, hardHatUpper, hardHatMask);

            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(hardHatMask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            double maxArea = 0;
            Rect hardHatRect = new Rect();

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > maxArea)
                {
                    maxArea = area;
                    hardHatRect = Cv2.BoundingRect(contour);
                }
            }

            bool hardHatDetected = maxArea > 3000;
            return (hardHatDetected, hardHatRect);
        }
    }
}
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using PPE_Project.Database;
using PPE_Project.Models;
using PPE_Project.Recognition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PPE_Project.Pages
{
    public partial class MainPage : Page
    {
        private VideoCapture capture;
        private Thread cameraThread;
        private bool isRunning = false;
        private CascadeClassifier faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
        private FaceRecognizer faceRecognizer = new FaceRecognizer();
        private PPEDetector ppeDetector = new PPEDetector();
        private ObservableCollection<AttendanceRecord> attendanceList = new ObservableCollection<AttendanceRecord>();
        private string currentMode = "출근";
        private Dictionary<string, bool> recordedEmployees = new Dictionary<string, bool>();

        public MainPage()
        {
            InitializeComponent();
            AttendanceList.ItemsSource = attendanceList;
            faceRecognizer.Train();
            StartCamera();
        }

        private void StartCamera()
        {
            capture = new VideoCapture(0);
            isRunning = true;

            cameraThread = new Thread(() =>
            {
                Mat frame = new Mat();

                while (isRunning)
                {
                    capture.Read(frame);

                    if (!frame.Empty())
                    {
                        var gray = new Mat();
                        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                        Cv2.EqualizeHist(gray, gray);

                        var faces = faceCascade.DetectMultiScale(gray, 1.1, 5);

                        string resultText = "";

                        foreach (var face in faces)
                        {
                            Cv2.Rectangle(frame, face, Scalar.Red, 2);

                            Mat faceROI = new Mat(frame, face);
                            var (employeeId, confidence) = faceRecognizer.Predict(faceROI);

                            if (confidence < 80 && employeeId != -1)
                            {
                                string name = faceRecognizer.GetName(employeeId);
                                resultText = $"{name} ({confidence:F1})";

                                if (!recordedEmployees.ContainsKey(name))
                                {
                                    recordedEmployees[name] = true;

                                    if (currentMode == "출근")
                                    {
                                        string ppeStatus = "미검사";

                                        if (ppeDetector.IsPPEEnabled())
                                        {
                                            var (hardHat, hardHatRect) = ppeDetector.Detect(frame);

                                            if (hardHatRect.Width > 0)
                                                Cv2.Rectangle(frame, hardHatRect, Scalar.Green, 2);

                                            if (hardHat)
                                                ppeStatus = "착용";
                                            else
                                                ppeStatus = "안전모 미착용";
                                        }

                                        Dispatcher.Invoke(() =>
                                        {
                                            attendanceList.Add(new AttendanceRecord
                                            {
                                                Name = name,
                                                Time = DateTime.Now.ToString("HH:mm:ss"),
                                                Status = "출근",
                                                PPEStatus = ppeStatus
                                            });
                                            DB.AddAttendance(name, "출근", ppeStatus);  // DB에 저장
                                        });
                                    }
                                    else
                                    {
                                        // 퇴근 모드
                                        Dispatcher.Invoke(() =>
                                        {
                                            attendanceList.Add(new AttendanceRecord
                                            {
                                                Name = name,
                                                Time = DateTime.Now.ToString("HH:mm:ss"),
                                                Status = "퇴근",
                                                PPEStatus = "-"
                                            });
                                            DB.AddAttendance(name, "퇴근", "-");  // DB에 저장
                                        });
                                    }
                                }
                            }
                            else
                            {
                                resultText = "알 수 없음";
                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            CameraView.Source = frame.ToBitmapSource();
                            RecognitionResult.Text = resultText;
                        });
                    }
                }
            });

            cameraThread.IsBackground = true;
            cameraThread.Start();
        }

        private void btn_CheckIn_Click(object sender, RoutedEventArgs e)
        {
            currentMode = "출근";
            recordedEmployees.Clear();
            CurrentMode.Text = "현재: 출근 모드";
        }

        private void btn_CheckOut_Click(object sender, RoutedEventArgs e)
        {
            currentMode = "퇴근";
            recordedEmployees.Clear();
            CurrentMode.Text = "현재: 퇴근 모드";
        }

        private void btn_PPE_Click(object sender, RoutedEventArgs e)
        {
            bool isEnabled = ppeDetector.TogglePPE();
            btn_PPE.Content = isEnabled ? "PPE 검사 ON" : "PPE 검사 OFF";
            btn_PPE.Background = isEnabled
                ? System.Windows.Media.Brushes.LightGreen
                : System.Windows.Media.Brushes.LightGray;
        }

    }
}
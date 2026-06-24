using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using PPE_Project.Database;
using PPE_Project.Models;

namespace PPE_Project.Pages
{
    public partial class EmployeePage : Page
    {
        private Employee selectedEmployee;
        private VideoCapture capture;
        private Thread cameraThread;
        private bool isRunning = false;
        private Mat currentFrame = new Mat();        // 현재 카메라 프레임
        private Mat capturedFrame = null;            // 캡처된 프레임
        private Mat latestFace = null;               // 스레드에서 감지한 최신 얼굴
        private CascadeClassifier faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

        public EmployeePage()
        {
            InitializeComponent();
            LoadEmployees();
            StartCamera();
        }

        private void LoadEmployees()
        {
            EmployeeListView.ItemsSource = DB.GetEmployees();
        }

        private void StartCamera()
        {
            capture = new VideoCapture(0);
            isRunning = true;

            cameraThread = new Thread(() =>
            {
                while (isRunning)
                {
                    capture.Read(currentFrame);

                    if (!currentFrame.Empty())
                    {
                        var gray = new Mat();
                        Cv2.CvtColor(currentFrame, gray, ColorConversionCodes.BGR2GRAY);
                        Cv2.EqualizeHist(gray, gray);
                        var faces = faceCascade.DetectMultiScale(gray, 1.1, 5);

                        Mat display = currentFrame.Clone();

                        if (faces.Length > 0)
                        {
                            var faceRect = faces[0];
                            Cv2.Rectangle(display, faceRect, Scalar.Green, 2);  // 초록 사각형

                            int padding = faceRect.Width / 3;
                            int x = Math.Max(0, faceRect.X - padding);
                            int y = Math.Max(0, faceRect.Y - padding);
                            int width = Math.Min(currentFrame.Width - x, faceRect.Width + padding * 2);
                            int height = Math.Min(currentFrame.Height - y, faceRect.Height + padding * 2);
                            var expandedRect = new OpenCvSharp.Rect(x, y, width, height);
                            latestFace = new Mat(currentFrame, expandedRect);  // 최신 얼굴 업데이트
                        }

                        Dispatcher.Invoke(() =>
                        {
                            CameraView.Source = display.ToBitmapSource();
                        });
                    }
                }
            });

            cameraThread.IsBackground = true;
            cameraThread.Start();
        }

        private void btn_Capture_Click(object sender, RoutedEventArgs e)
        {
            if (latestFace == null) return;

            capturedFrame = latestFace.Clone();

            Dispatcher.Invoke(() =>
            {
                FaceImage.Source = capturedFrame.ToBitmapSource();
            });
        }

        private void EmployeeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedEmployee = EmployeeListView.SelectedItem as Employee;

            if (selectedEmployee != null)
            {
                NameTextBox.Text = selectedEmployee.Name;
                PositionTextBox.Text = selectedEmployee.Position;

                if (selectedEmployee.FaceData != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(selectedEmployee.FaceData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    FaceImage.Source = bitmap;
                }
            }
        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text)) return;

            byte[] faceData = null;

            if (capturedFrame != null)
            {
                Cv2.ImEncode(".jpg", capturedFrame, out byte[] buf);
                faceData = buf;
            }

            DB.AddEmployee(NameTextBox.Text, PositionTextBox.Text, faceData);
            LoadEmployees();
        }

        private void btn_Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEmployee == null) return;

            DB.UpdateEmployee(selectedEmployee.Id, NameTextBox.Text, PositionTextBox.Text);
            LoadEmployees();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEmployee == null) return;

            DB.DeleteEmployee(selectedEmployee.Id);
            LoadEmployees();
        }
    }
}
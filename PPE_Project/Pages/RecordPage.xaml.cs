using System;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PPE_Project.Database;
using PPE_Project.Models;

namespace PPE_Project.Pages
{
    public partial class RecordPage : Page
    {
        public RecordPage()
        {
            InitializeComponent();
            LoadRecords();
        }

        private void LoadRecords(string searchName = "")
        {
            RecordListView.ItemsSource = DB.GetAttendance(searchName);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadRecords(SearchBox.Text);
        }

        private void RecordListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = RecordListView.SelectedItem as AttendanceRecord;
            if (selected == null) return;

            string name = selected.Name;
            StatTitle.Text = $"{name} 사원의 {DateTime.Now.Month}월 통계";

            // 출근 일수
            int days = DB.GetEmployeeAttendanceDays(name);
            AttendanceDays.Text = $"{days}일";

            // PPE 착용률 파이차트
            var (worn, notWorn, notChecked) = DB.GetEmployeePPEStats(name);
            int total = worn + notWorn + notChecked;

            PPEChart.Series = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { worn },
                    Name = "착용",
                    Fill = new SolidColorPaint(SKColors.Green),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    HoverPushout = 0,
                    DataLabelsFormatter = p => worn == 0 ? "" : $"{(total > 0 ? (worn * 100 / total) : 0)}%"
                },
                new PieSeries<double>
                {
                    Values = new double[] { notWorn },
                    Name = "미착용",
                    Fill = new SolidColorPaint(SKColors.Red),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    HoverPushout = 0,
                    DataLabelsFormatter = p => notWorn == 0 ? "" : $"{(total > 0 ? (notWorn * 100 / total) : 0)}%"
                },
                new PieSeries<double>
                {
                    Values = new double[] { notChecked },
                    Name = "미검사",
                    Fill = new SolidColorPaint(SKColors.Gray),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    HoverPushout = 0,
                    DataLabelsFormatter = p => notChecked == 0 ? "" : $"{(total > 0 ? (notChecked * 100 / total) : 0)}%"
                }
            };
        }
    }
}
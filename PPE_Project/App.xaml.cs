using System.Configuration;
using System.Data;
using System.Windows;
using PPE_Project.Database;

namespace PPE_Project
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);     // 부모 클래스(Application)의 원래 동작도 실행
            DB.Initialize();       // DB 초기화 (테이블 생성)
        }
    }

}

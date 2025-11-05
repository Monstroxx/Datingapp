using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace BananaLove
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //delete debug.log on start
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (File.Exists("debug.log"))
            {
                File.Delete("debug.log");
            }
        }
    }

}

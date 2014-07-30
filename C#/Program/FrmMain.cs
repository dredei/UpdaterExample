#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace Program
{
    public partial class FrmMain : Form
    {
        private readonly Version _version = new Version( 1, 0, 0 );
        private const string VersionFileLink = "https://github.com/dredei/UpdaterExample/raw/master/version.txt";
        private const string UpdaterName = "Updater";

        public FrmMain()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Запускает программу для проверки обновлений
        /// </summary>
        /// <returns></returns>
        private async Task CheckForUpdates()
        {
            string updaterFileName = Application.StartupPath + "\\" + UpdaterName + ".exe";
            if ( !File.Exists( updaterFileName ) )
            {
                return;
            }
            // 
            Process.Start( updaterFileName, this._version + " " + VersionFileLink );
            await TaskEx.Delay( 1500 );
            Process[] updater = Process.GetProcessesByName( UpdaterName );
            if ( updater.Length == 0 )
            {
                return;
            }
            // Ожидаем завершения Updater'а
            updater[ 0 ].WaitForExit();
        }

        private async void FrmMain_Load( object sender, EventArgs e )
        {
            await this.CheckForUpdates();
            this.lblVersion.Text = this._version.ToString();
        }
    }
}
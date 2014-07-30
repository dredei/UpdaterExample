#region Using

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace Updater
{
    public partial class FrmMain : Form
    {
        private readonly Updater _updater;

        public FrmMain( string[] args )
        {
            this.InitializeComponent();
            if ( args.Length == 2 )
            {
                Version version = null;
                string fileLink = string.Empty;
                try
                {
                    version = new Version( args[ 0 ] );
                    fileLink = args[ 1 ];
                }
                catch ( Exception )
                {
                    Application.Exit();
                }
                this._updater = new Updater( version, fileLink );
            }
        }

        private async Task CheckForUpdates()
        {
            try
            {
                await this._updater.CheckForUpdates();
            }
            finally
            {
                Application.Exit();
            }
        }

        private async void FrmMain_Load( object sender, EventArgs e )
        {
            await this.CheckForUpdates();
        }

        private void tmrUpdProgress_Tick( object sender, EventArgs e )
        {
            if ( this._updater == null )
            {
                return;
            }
            this.pb1.Value = this._updater.CurrentUpdProgress;
        }
    }
}
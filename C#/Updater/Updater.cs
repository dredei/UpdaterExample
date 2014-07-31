#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Updater
{
    internal class Updater
    {
        private readonly Version _currentVersion;
        private readonly string _fileLink;
        private readonly WebClient _webClient;
        private bool _isDownloaded;

        public int CurrentUpdProgress { get; private set; }

        public Updater( Version currentVersion, string fileLink )
        {
            this._currentVersion = currentVersion;
            this._fileLink = fileLink;
            this._webClient = new WebClient { Encoding = Encoding.UTF8 };
            this._webClient.DownloadProgressChanged += this._webClient_DownloadProgressChanged;
            this._webClient.DownloadFileCompleted += this._webClient_DownloadFileCompleted;
        }

        /// <summary>
        /// Срабатывает, когда файл скачался
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _webClient_DownloadFileCompleted( object sender, System.ComponentModel.AsyncCompletedEventArgs e )
        {
            this._isDownloaded = true;
        }

        /// <summary>
        /// Срабатывает во время изменения прогресса скачивания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _webClient_DownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
        {
            this.CurrentUpdProgress = e.ProgressPercentage;
        }

        /// <summary>
        /// Ассинхронное скачивание строки
        /// </summary>
        /// <param name="url">Ссылка</param>
        /// <returns></returns>
        private async Task<string> DownloadStringAsync( string url )
        {
            return await TaskEx.Run( () =>
            {
                string result;
                try
                {
                    result = this._webClient.DownloadString( url );
                }
                catch
                {
                    result = "";
                }
                return result;
            } );
        }

        /// <summary>
        /// Получает ссылку на скачивание последней версии
        /// </summary>
        /// <returns>Возвращает ссылку на последнюю версию или пустую строку, если обновление не требуется</returns>
        private async Task<string> GetReleaseLink()
        {
            string fileContent = await this.DownloadStringAsync( this._fileLink );
            if ( string.IsNullOrEmpty( fileContent ) )
            {
                return string.Empty;
            }
            // парсим строку текстового файла
            string[] dataArr = fileContent.Split( ';' );
            var fileVersion = new Version( dataArr[ 0 ] );
            string releaseLink = dataArr[ 1 ];
            return this._currentVersion >= fileVersion ? "" : releaseLink;
        }

        /// <summary>
        /// Проверяет обновления
        /// </summary>
        /// <returns></returns>
        public async Task CheckForUpdates()
        {
            // получаем ссылку на последнюю версию
            string releaseLink = await this.GetReleaseLink();
            if ( string.IsNullOrEmpty( releaseLink ) )
            {
                return;
            }
            // получаем путь для сохранения инсталлятора
            string fileName = Path.GetTempPath() + "updater_example_setup.exe";
            this._isDownloaded = false;
            try
            {
                this._webClient.DownloadFileAsync( new Uri( releaseLink ), fileName );
            }
            catch
            {
                return;
            }
            Process[] waControllers = Process.GetProcessesByName( "Program" );
            // закрываем экземпляры нашей программы
            Array.ForEach( waControllers, p => p.CloseMainWindow() );
            while ( !this._isDownloaded )
            {
                await TaskEx.Delay( 100 );
            }
            // запускаем инсталлятор с параметром /VERYSILENT
            Process.Start( fileName, "/VERYSILENT" );
        }
    }
}
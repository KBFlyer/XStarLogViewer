using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
namespace AutelXSPLogViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //public static LogBrowse mainForm = new LogBrowse();
        static void Main()
        {
            
            // set the cache provider to my custom version
            GMap.NET.GMaps.Instance.PrimaryCache = new Maps.MyImageCache();
            // add my custom map providers
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.WMSProvider.Instance);
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.Custom.Instance);
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.Earthbuilder.Instance);
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.Statkart_Topo2.Instance);
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.MapBox.Instance);
            GMap.NET.MapProviders.GMapProviders.List.Add(Maps.MapboxNoFly.Instance);

            // add proxy settings
            GMap.NET.MapProviders.GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMap.NET.MapProviders.GMapProvider.WebProxy.Credentials = CredentialCache.DefaultCredentials;

            //WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            //WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;


            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LogBrowse mainForm = new LogBrowse();
            Application.Run(mainForm);
        }
    }
}

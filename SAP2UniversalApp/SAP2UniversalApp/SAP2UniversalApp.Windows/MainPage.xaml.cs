using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SAP2UniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SAPGWM_Click(object sender, RoutedEventArgs e)
        {
            if (!AADAuthHelper.Instance.IsAuthenticated)
            {
                HelpText.Visibility = Visibility.Collapsed;
                SignInPage.Navigate(new Uri(AADAuthHelper.Instance.AuthorizeUrl));
            }
            else
            {
                await GetData();
            }
        }

        private async void SignInPage_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.AbsoluteUri.StartsWith(AADAuthHelper.AppRedirectUrl))
            {
                sender.Stop();

                var querys = args.Uri.Query.Substring(1).Split('&');
                string code = null;
                foreach (string query in querys)
                {
                    if (query.StartsWith("code="))
                    {
                        code = query.Substring(5);
                        break;
                    }
                }

                if (string.IsNullOrEmpty(code)) throw new UnauthorizedAccessException();

                await AADAuthHelper.Instance.AcquireTokenWithAuthorizeCode(code);
                await GetData();
            }
        }

        private async Task<IAsyncAction> GetData()
        {
            DataModel[] gwmData = await GWMDataHelper.Instance.GetGWMData();
            return this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                SignInPage.Visibility = Visibility.Collapsed;
                DataList.DataContext = new ObservableCollection<DataModel>(gwmData);
            });
        }
    }
}

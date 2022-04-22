using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Net;
using  System.Net.Http;
using System.IO;
using RestSharp;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Storyboard _hideClockFaceStoryboard;
        private Storyboard _hideWeatherImageStoryboard;
        // Storyboards for the animations.
        private Storyboard _showClockFaceStoryboard;
        private Storyboard _showWeatherImageStoryboard;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load the storyboard resources.
            _showClockFaceStoryboard =
                (Storyboard)Resources["ShowClockFaceStoryboard"];
            _hideClockFaceStoryboard =
                (Storyboard)Resources["HideClockFaceStoryboard"];
            _showWeatherImageStoryboard =
                (Storyboard)Resources["ShowWeatherImageStoryboard"];
            _hideWeatherImageStoryboard =
                (Storyboard)Resources["HideWeatherImageStoryboard"];
        }

       
        private void ForecastButtonHandler(object sender, RoutedEventArgs e)
        {
            // Change the status image and start the rotation animation.
            fetchButton.IsEnabled = false;
            fetchButton.Content = "Contacting Server";
            weatherText.Text = "";
            _hideWeatherImageStoryboard.Begin(this);

            // Start fetching the weather forecast asynchronously.
            var fetcher = new NoArgDelegate(
                FetchWeatherFromServer);

            fetcher.BeginInvoke(null, null);
        }

        private async void FetchWeatherFromServer()
        {
            Thread.Sleep(2000);
            var client = new RestClient("https://api.weather.gov/gridpoints/TOP/31,80/forecast");

            var request = new RestRequest();
            var response = await client.ExecuteGetAsync(request);
            Console.WriteLine(response.ResponseStatus.ToString());

            JObject json = JObject.Parse(response.Content.ToString());

            var x = JsonConvert.DeserializeObject<JObject>(response.Content.ToString());

            int geography = new Random().Next(13);
            var xx = x["properties"]["periods"][geography]["shortForecast"];

            tomorrowsWeather.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new OneArgDelegate(UpdateUserInterface),
                xx.ToString());
        }

        private void UpdateUserInterface(string res)
        {
            //Set the weather image
            if (res.Contains("Cloudy") || res.Contains("rainy")
                || res.Contains("Showers")
                || res.Contains("rainy")
                || res.Contains("Drizzle"))
            {
                weatherIndicatorImage.Source = (ImageSource)Resources[
                    "RainingImageSource"];
              
            }
            else 
            {
                weatherIndicatorImage.Source = (ImageSource)Resources[
                  "SunnyImageSource"];
            }

            //Stop clock animation
            _showClockFaceStoryboard.Stop(this);
            _hideClockFaceStoryboard.Begin(this);

            //Update UI text
            fetchButton.IsEnabled = true;
            fetchButton.Content = "Fetch Forecast";
            weatherText.Text = res;
        }

        private void HideClockFaceStoryboard_Completed(object sender,
            EventArgs args)
        {
            _showWeatherImageStoryboard.Begin(this);
        }

        private void HideWeatherImageStoryboard_Completed(object sender,
            EventArgs args)
        {
            _showClockFaceStoryboard.Begin(this, true);
        }

        private delegate void NoArgDelegate();

        private delegate void OneArgDelegate(string arg);
    }
}

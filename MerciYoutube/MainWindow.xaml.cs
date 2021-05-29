using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Timer = System.Windows.Forms.Timer;
using System.Threading.Tasks;

namespace MerciYoutube
{
    public partial class MainWindow : Window
    {
        //Variables
        private string channelName = "YOUR CHANNEL NAME";
        private List<Subscriber> subscribers;
        private int currSubscriberIndex = -1;
        private Timer timer;
        private int intervall = 3000; 
        private SpeechSynthesizer synth = new SpeechSynthesizer();

        //Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Get subscribers of channel
            this.subscribers = GetSubscribers().Result;
            
            // Initialize timer
            this.timer = new Timer()
            {
                Interval = this.intervall
            };
            this.timer.Tick += ThanksAnother;
            this.timer.Start();

            // Set audio device of speech synthesizer
            synth.SetOutputToDefaultAudioDevice();
        }

        // Thanks next subscriber
        private void ThanksAnother(object sender, EventArgs e)
        {
            // Set next subscriber index
            currSubscriberIndex++;
            if(currSubscriberIndex >= subscribers.Count)
            {
                this.timer.Stop();
                return;
            }

            // Get subscriber
            Subscriber currSub = subscribers[currSubscriberIndex];

            // Get his logo
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(currSub.IconUrl, UriKind.Absolute);
            bitmap.EndInit();

            // Show subscriber
            this.LB_SubName.Content = currSub.Name;
            this.Img_subLogo.Source = bitmap;

            // Speak "Thanks"
            Speak(currSub.Name);
        }

        // Speak thanks [subscriber name]
        private void Speak(string name)
        {
            // Cut name if it's long
            string subName = name;
            if(subName.Length > 25)
            {
                subName = name.Substring(0, 25);
            }

            // Start speak on another thread
            Thread thread = new Thread(new ThreadStart(() => synth.Speak("Merci " + subName)));
            thread.Start();
        }

        // Get subscribers of a channel
        private async Task<List<Subscriber>> GetSubscribers()
        {
            // Create crendential from json file (Get it from console.cloud.google.com/api/credentials)
            UserCredential credential;
            using (var stream = new FileStream("auth.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    channelName,
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            // Create Youtube service
            YouTubeService youtube = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName= this.GetType().ToString()
            });

            // Get subscribers list
            SubscriptionsResource.ListRequest slr = youtube.Subscriptions.List("subscriberSnippet");
            slr.MySubscribers = true;
            slr.MaxResults = 50;

            // Execute query
            SubscriptionListResponse subscribers;

            // Get data
            List<Subscriber> result = new List<Subscriber>();
            do
            {
                subscribers = slr.Execute();
                foreach (var item in subscribers.Items)
                {
                    Subscriber s = new Subscriber(item.SubscriberSnippet.Title, item.SubscriberSnippet.Thumbnails.Medium.Url);
                    result.Add(s);
                }

                slr.PageToken = subscribers.NextPageToken;
            } while (!string.IsNullOrEmpty(slr.PageToken));

            return result;
        }
    }
}

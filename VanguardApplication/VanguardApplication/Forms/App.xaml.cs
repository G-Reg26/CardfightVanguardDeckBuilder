using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VanguardApplication
{
    public partial class App : Application
    {
        public static List<Deck> Decks;
        public static Deck CurrentDeck;

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new StartPage());
        }

        protected override void OnStart()
        {
            Decks = GetDecks();

            CurrentDeck = Decks.Count == 0 ? null : Decks[0];
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        public static async Task<HtmlDocument> HTMLParsing(string url)
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                return htmlDoc;
            }
        }

        public static List<Deck> GetDecks()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string filePath = Path.Combine(path, "decks.txt");
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    List<Deck> decks = JsonConvert.DeserializeObject<List<Deck>>(json);

                    return decks;
                }
            }
            catch (Exception)
            {
                return new List<Deck>();
            }
        }

        public static void SaveDecks()
        {
            string json = JsonConvert.SerializeObject(Decks);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(path, "decks.txt");
            using (var file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (var w = new StreamWriter(file))
            {
                w.Write(json);
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VanguardApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardListView : ContentPage
    {
        Deck deck;

        private readonly int maxPages;
        private readonly string URL;

        private int currentPage;
        private bool loadingCards;
        private bool cardSelected;

        public ObservableCollection<CardListing> Items { get; set; }

        public CardListView(int maxPages, string URL, Deck deck)
        {
            InitializeComponent();

            Title = "Search Results";

            this.maxPages = maxPages;
            this.URL = URL;
            this.deck = deck;

            cardSelected = false;
            currentPage = 1;

            Items = new ObservableCollection<CardListing>();

            MyListView.ItemsSource = Items;
            MyListView.ItemAppearing += Item_Appears;

            LoadCardListings();
        }

        private void Item_Appears(object sender, ItemVisibilityEventArgs e)
        {
            if (loadingCards || currentPage > maxPages)
            {
                return;
            }

            if (e.Item == Items[Items.Count - 1])
            {
                LoadCardListings();
            }
        }

        private async void LoadCardListings()
        {
            Loading.IsRunning = true;
            loadingCards = true;

            var baseURL = URL;
            
            string url = baseURL + "&page=" + currentPage;
            var htmlDoc = await App.HTMLParsing(url.ToString());

            var cardContainer = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("id", "")
                .Equals("cardlist-container")).ToList();

            var cardGallery = cardContainer[0].Descendants("li").ToList();

            foreach (var card in cardGallery)
            {
                string cardURL;
                string title;
                string imageURL;

                cardURL = card.FirstChild.GetAttributeValue("href", "");
                title = card.FirstChild.FirstChild.GetAttributeValue("title", "");
                imageURL = card.FirstChild.FirstChild.GetAttributeValue("src", "");

                title = title.Replace("&amp;#9829", "&#9829;");
                title = title.Replace("&amp;hearts;", "&#9829;");
                
                title = System.Net.WebUtility.HtmlDecode(title);

                Items.Add(new CardListing() { Title = title, URL = cardURL, Image = imageURL });
            }

            currentPage++;

            loadingCards = false;
            Loading.IsRunning = false;
        }

        static async Task<Card> GetCardInformation(string URL)
        {
            Card card = new Card();

            var htmlDoc = await App.HTMLParsing("https://en.cf-vanguard.com" + URL);

            var set = htmlDoc.DocumentNode.Descendants("h3")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("style-h3")).ToList();

            card.Set = set[0].InnerText;

            var image = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("image")).ToList();

            card.Image = image[0].ChildNodes[1].FirstChild.GetAttributeValue("src", "");

            var data = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("data")).ToList();

            var name = data[0].ChildNodes[1].ChildNodes[1].InnerText;
            name = name.Replace("&#9829", "&#9829;");

            card.Name = System.Net.WebUtility.HtmlDecode(name);

            var effect = data[0].ChildNodes[7].InnerHtml;
            effect = effect.Replace("<br>", "\n");
            effect = effect.Replace("&#9829", "&#9829;");

            card.Effect = System.Net.WebUtility.HtmlDecode(effect);

            card.Effect = card.Effect.Replace("&lt", "<");
            card.Effect = card.Effect.Replace("&gt", ">");

            var flavor = data[0].ChildNodes[9].InnerText;
            flavor = flavor.Replace("&#9829", "&#9829;");

            card.Flavor = System.Net.WebUtility.HtmlDecode(flavor);

            card.Flavor = card.Flavor.Replace("&lt", "<");
            card.Flavor = card.Flavor.Replace("&gt", ">");

            var textList = data[0].Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("text-list")).ToList();

            card.Type = textList[0].ChildNodes[1].InnerText;
            card.Group = textList[0].ChildNodes[3].InnerText;
            card.Race = textList[0].ChildNodes[5].InnerText;
            card.Nation = textList[0].ChildNodes[7].InnerText;
            card.Grade = textList[0].ChildNodes[9].InnerText;
            card.Power = textList[0].ChildNodes[11].InnerText;
            card.Critical = textList[0].ChildNodes[13].InnerText;
            card.Shield = textList[0].ChildNodes[15].InnerText;
            card.Skill = textList[0].ChildNodes[17].InnerText;
            card.Gift = textList[0].ChildNodes[19].InnerText;

            card.Regulation = textList[1].ChildNodes[1].InnerText.Replace("\n", "");
            card.Number = textList[1].ChildNodes[3].InnerText;
            card.Rarity = textList[1].ChildNodes[5].InnerText;
            card.Illustrator = textList[1].ChildNodes[7].InnerText;

            return card;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            if (!cardSelected)
            {
                cardSelected = true;
                Loading.IsRunning = true;

                Card card = await GetCardInformation(((CardListing)e.Item).URL);

                await Navigation.PushAsync(new CardView(card, deck));

                Loading.IsRunning = false;
                cardSelected = false;
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}

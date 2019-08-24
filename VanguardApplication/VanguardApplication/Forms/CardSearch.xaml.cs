using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Threading;

namespace VanguardApplication
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class CardSearch : ContentPage
    {
        private CancellationTokenSource cancelSearchToken;

        private Deck deck;

        private bool comingFromDeck;
        private bool cancelSearch;
        private bool searching;

        public CardSearch(bool comingFromDeck, Deck deck)
        {
            InitializeComponent();

            this.comingFromDeck = comingFromDeck;
            this.deck = deck;

            cancelSearch = false;
            searching = false;

            CancelSearch.IsEnabled = false;

            // set regulation picker to deafult (All)
            Regulation.SelectedIndex = 0;

            // set keyword type to default (All)
            KeywordTypeAll.BorderColor = Color.Red;

            // peform a click on advance
            Advance_Search_Clicked(AdvSearch, new EventArgs());
        }

        protected override void OnAppearing()
        {
            Loading.IsRunning = false;
        }

        protected override void OnDisappearing()
        {
            if (comingFromDeck && Submit.IsEnabled)
            {
                Loading.IsRunning = true;
            }
        }

        private void Advance_Search_Clicked(object sender, EventArgs e)
        {
            // change button text
            AdvSearch.Text = AdvSearch.Text == "Advance Search" ?  "Normal Search" : "Advance Search";

            // set all advance button selections to default
            Unit_Type_Clicked(UnitTypeAll, e);
            Grade_Clicked(AllGrades, e);
            Trigger_Clicked(AllTriggers, e);

            // set all advance picker options to default
            Clan.SelectedIndex = 0;
            PowerFrom.SelectedIndex = 0;
            PowerTo.SelectedIndex = 0;
            Rarity.SelectedIndex = 0;

            // hide or reveal advance search options
            ClanLabel.IsVisible = !ClanLabel.IsVisible;
            Clan.IsVisible = !Clan.IsVisible;

            UnitLabel.IsVisible = !UnitLabel.IsVisible;
            UnitTypGrid.IsVisible = !UnitTypGrid.IsVisible;

            GradeLabel.IsVisible = !GradeLabel.IsVisible;
            GradeGrid.IsVisible = !GradeGrid.IsVisible;

            PowerLabel.IsVisible = !PowerLabel.IsVisible;
            PowerGrid.IsVisible = !PowerGrid.IsVisible;

            RarityLabel.IsVisible = !RarityLabel.IsVisible;
            Rarity.IsVisible = !Rarity.IsVisible;

            TriggerLabel.IsVisible = !TriggerLabel.IsVisible;
            TriggerGrid.IsVisible = !TriggerGrid.IsVisible;
        }

        private void Keyword_Type_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.BorderColor == Color.Default)
            {
                button.BorderColor = Color.Red;
            }
            else
            {
                if (button != KeywordTypeAll)
                {
                    button.BorderColor = Color.Default;
                }
            }

            if (button == KeywordTypeAll)
            {
                KeywordTypeName.BorderColor = Color.Default;
                KeywordTypeText.BorderColor = Color.Default;
                KeywordTypeCardNo.BorderColor = Color.Default;
                KeywordTypeRace.BorderColor = Color.Default;
                KeywordTypeIllu.BorderColor = Color.Default;
            }
            else
            {
                KeywordTypeAll.BorderColor = Color.Default;
            }
        }
                
        private void Unit_Type_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.BorderColor == Color.Default)
            {
                button.BorderColor = Color.Red;
            }
            else
            {
                if (button != UnitTypeAll)
                {
                    button.BorderColor = Color.Default;
                }
            }

            if (button == UnitTypeAll)
            {
                Normal.BorderColor = Color.Default;
                Trigger.BorderColor = Color.Default;
                G.BorderColor = Color.Default;
                UnitTypeOther.BorderColor = Color.Default;
            }
            else
            {
                UnitTypeAll.BorderColor = Color.Default;
            }
        }

        private void Grade_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.BorderColor == Color.Default)
            {
                button.BorderColor = Color.Red;
            }
            else
            {
                if (button != AllGrades)
                {
                    button.BorderColor = Color.Default;
                }
            }

            if (button == AllGrades)
            {
                Grade0.BorderColor = Color.Default;
                Grade1.BorderColor = Color.Default;
                Grade2.BorderColor = Color.Default;
                Grade3.BorderColor = Color.Default;
                Grade4.BorderColor = Color.Default;
                OtherGrades.BorderColor = Color.Default;
            }
            else
            {
                AllGrades.BorderColor = Color.Default;
            }
        }

        private void Trigger_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.BorderColor == Color.Default)
            {
                button.BorderColor = Color.Red;
            }
            else
            {
                if (button != AllTriggers)
                {
                    button.BorderColor = Color.Default;
                }
            }

            if (button == AllTriggers)
            {
                Stand.BorderColor = Color.Default;
                Heal.BorderColor = Color.Default;
                Draw.BorderColor = Color.Default;
                Critical.BorderColor = Color.Default;
                Front.BorderColor = Color.Default;
            }
            else
            {
                AllTriggers.BorderColor = Color.Default;
            }
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            CancelSearch.IsEnabled = true;

            Submit.Text = "Searching";
            Submit.IsEnabled = false;

            var URL = ConstructURL();
            int maxPages = await GetMaxPages(URL);

            if (maxPages != 0)
            {
                await Navigation.PushAsync(new CardListView(maxPages, URL, deck));
            }

            Submit.Text = "Submit";
            Submit.IsEnabled = true;

            CancelSearch.IsEnabled = false;
            cancelSearch = false;
        }

        private void Cancel_Search_Clicked(object sender, EventArgs e)
        {
            if (cancelSearchToken != null)
            {
                cancelSearch = true;
                cancelSearchToken.Cancel();
                cancelSearchToken = null;
            }
        }

        private async Task<int> GetMaxPages(string URL)
        {
            searching = true;
            int maxPage = 0;

            if (cancelSearchToken == null)
            {
                cancelSearchToken = new CancellationTokenSource();
            }

            Device.StartTimer(new TimeSpan(0, 0, 0, 0, 500), Searching_Animation);

            try
            {
                cancelSearchToken.Token.ThrowIfCancellationRequested();
                var htmlDoc = await App.HTMLParsing(URL);

                // insert all script nodes in html doc into a list
                cancelSearchToken.Token.ThrowIfCancellationRequested();
                var scriptNodes = htmlDoc.DocumentNode.Descendants("script")
                    .Where(node => node.GetAttributeValue("type", "")
                    .Equals("text/javascript")).ToList();

                int begin = 0;

                foreach (var scriptNode in scriptNodes)
                {
                    cancelSearchToken.Token.ThrowIfCancellationRequested();
                    // ex:
                    // initial string: [...var max_page = 357;...]
                    // move begin index: ...var [max_page = 357;...]
                    begin = scriptNode.InnerText.ToString().IndexOf("max_page");

                    // found chunk of html that contains the variable max_page
                    if (begin != -1)
                    {
                        // move begin index: var max_page = [357;...]
                        begin += 11;

                        // create seperate string: [357;...]
                        string subStr = scriptNode.InnerText.ToString().Substring(begin);

                        // move end index: [357];...
                        // parse string into integer
                        maxPage = Int32.Parse(subStr.Substring(0, subStr.IndexOf(";")));

                        break;
                    }
                }

                if (begin == -1)
                { 
                    await DisplayAlert("No Results Found", "No cards found.", "OK");
                }

                searching = false;
                return maxPage;
            }
            catch (Exception)
            {
                if (!cancelSearch)
                {
                    await DisplayAlert("Search Failed", "HTTP failed to send request.", "OK");
                }

                searching = false;
                return 0;
            }
        }

        private bool Searching_Animation()
        {
            Device.BeginInvokeOnMainThread(() => {
                if (Submit.Text == "Searching...")
                {
                    Submit.Text = "Searching";
                }
                else
                {
                    if (Submit.Text != "Submit")
                    {
                        Submit.Text += ".";
                    }
                }
            });
            return searching;
        }

        private string ConstructURL()
        {
            var url = "https://en.cf-vanguard.com/cardlist/cardsearch/";

            var reg = "?regulation%5B%5D=";

            switch (Regulation.SelectedIndex)
            {
                case 0:
                    reg += "all";
                    break;
                case 1:
                    reg += "V";
                    break;
                case 2:
                    reg += "G";
                    break;
            }

            var keyword = "&keyword=" + Keyword.Text.ToString().Replace(' ', '+');

            var keywordType = "";

            if (KeywordTypeAll.BorderColor == Color.Red)
            {
                keywordType = "&keyword_type%5B%5D=all";
            }
            else
            {
                if (KeywordTypeName.BorderColor == Color.Red)
                {
                    keywordType += "&keyword_type%5B%5D=name";
                }
                if (KeywordTypeText.BorderColor == Color.Red)
                {
                    keywordType += "&keyword_type%5B%5D=text";
                }
                if (KeywordTypeCardNo.BorderColor == Color.Red)
                {
                    keywordType += "&keyword_type%5B%5D=no";
                }
                if (KeywordTypeRace.BorderColor == Color.Red)
                {
                    keywordType += "&keyword_type%5B%5D=tribe";
                }
                if (KeywordTypeIllu.BorderColor == Color.Red)
                {
                    keywordType += "&keyword_type%5B%5D=illustrator";
                }
            }

            var clan = "&clan=";

            if ((string)Clan.Items[Clan.SelectedIndex] != "All")
            {
                clan += (string)Clan.Items[Clan.SelectedIndex];
            }

            var kind = "";

            if (UnitTypeAll.BorderColor == Color.Red)
            {
                kind = "&kind%5B%5D=all";
            }
            else
            {
                if (Normal.BorderColor == Color.Red)
                {
                    kind += "&kind%5B%5D=1";
                }
                if (Trigger.BorderColor == Color.Red)
                {
                    kind += "&kind%5B%5D=2";
                }
                if (G.BorderColor == Color.Red)
                {
                    kind += "&kind%5B%5D=3";
                }
                if (UnitTypeOther.BorderColor == Color.Red)
                {
                    kind += "&kind%5B%5D=4";
                }
            }

            var grade = "";

            if (AllGrades.BorderColor == Color.Red)
            {
                grade = "&grade%5B%5D=all";
            }
            else
            {
                if (Grade0.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=0";
                }
                if (Grade1.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=1";
                }
                if (Grade2.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=2";
                }
                if (Grade3.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=3";
                }
                if (Grade4.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=4";
                }
                if (OtherGrades.BorderColor == Color.Red)
                {
                    grade += "&grade%5B%5D=other";
                }
            }

            var powerFrom = "&power_from=" + (string)PowerFrom.Items[PowerFrom.SelectedIndex];

            var powerTo = "&power_to=" + (string)PowerTo.Items[PowerFrom.SelectedIndex];

            var rare = "&rare=";

            if ((string)Rarity.Items[Rarity.SelectedIndex] != "All")
            {
                rare += (string)Rarity.Items[Rarity.SelectedIndex];
            }

            var trigger = "";

            if (AllTriggers.BorderColor == Color.Red)
            {
                trigger = "&trigger%5B%5D=all";
            }
            else
            {
                if (Stand.BorderColor == Color.Red)
                {
                    trigger += "&trigger%5B%5D=Stand";
                }
                if (Heal.BorderColor == Color.Red)
                {
                    trigger += "&trigger%5B%5D=Heal";
                }
                if (Draw.BorderColor == Color.Red)
                {
                    trigger += "&trigger%5B%5D=Draw";
                }
                if (Critical.BorderColor == Color.Red)
                {
                    trigger += "&trigger%5B%5D=Critical";
                }
                if (Front.BorderColor == Color.Red)
                {
                    trigger += "&trigger%5B%5D=Front";
                }
            }

            return url + reg + keyword + keywordType + clan + kind + grade + powerFrom + powerTo + rare + trigger;
        }
    }   
}

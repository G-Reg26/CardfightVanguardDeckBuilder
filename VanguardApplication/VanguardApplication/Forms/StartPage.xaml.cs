using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VanguardApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private async void Decks_Clicked(object sender, EventArgs e)
        {
            Decks.IsEnabled = false;

            await Navigation.PushAsync(new DecksList());

            Decks.IsEnabled = true;
        }

        private async void Card_Search_Clicked(object sender, EventArgs e)
        {
            CardSearch.IsEnabled = false;

            await Navigation.PushAsync(new CardSearch(false, null));

            CardSearch.IsEnabled = true;
        }
    }
}
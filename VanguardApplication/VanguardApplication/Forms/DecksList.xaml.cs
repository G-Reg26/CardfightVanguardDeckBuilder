using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VanguardApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DecksList : ContentPage
    {
        private bool cardSelected;

        public ObservableCollection<Deck> Items { get; set; }

        public DecksList()
        {
            InitializeComponent();

            Title = "Deck List";

            cardSelected = false;

            Items = new ObservableCollection<Deck>();

            MyListView.ItemsSource = Items;

            foreach (Deck deck in App.Decks)
            {
                Items.Add(deck);
            }
        }

        protected override void OnAppearing()
        {
            Loading.IsRunning = false;
        }

        protected override void OnDisappearing()
        {
            Loading.IsRunning = true;
        }

        void Remove_Deck_Clicked(object sender, EventArgs e)
        {
            RemoveDeck.Text = RemoveDeck.Text == "Done" ? "Remove Deck" : "Done";
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            if (!cardSelected)
            {
                cardSelected = true;

                if (RemoveDeck.Text == "Done")
                {
                    bool answer = await DisplayAlert(
                        "Remove Deck?",
                        "Would you like to remove " + ((Deck)e.Item).Name + "?",
                        "Yes",
                        "No");

                    if (answer)
                    {
                        Items.Remove((Deck)e.Item);
                        App.Decks.Remove((Deck)e.Item);

                        App.SaveDecks();
                    }
                }
                else
                {
                    Loading.IsRunning = true;
                    await Navigation.PushAsync(new DeckView((Deck)e.Item));
                    Loading.IsRunning = false;
                }

                cardSelected = false;
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        void Add_New_Deck_Clicked(object sender, EventArgs e)
        {
            NewDeckLayout.IsVisible = !NewDeckLayout.IsVisible;
            NewDeck.Text = NewDeck.Text == "New Deck" ? "Done" : "New Deck";
        }

        void Create_Deck_Clicked(object sender, EventArgs e)
        {
            // New deck entry is not an empty string
            if (NewDeckEntry.Text.Replace(" ", "") != "")
            {
                // Decks list does contains deck with new deck name
                if (App.Decks.Any(x => x.Name == NewDeckEntry.Text))
                {
                    DisplayAlert(
                        "Duplicate Deck Name",
                        "Deck name is already in use. Please enter new deck name",
                        "OK");

                    return;
                }

                Deck deck = new Deck() { Name = NewDeckEntry.Text };

                App.Decks.Add(deck);
                Items.Add(deck);

                App.SaveDecks();
            }
            else
            {
                DisplayAlert(
                    "Enter Deck Name",
                    "Please enter name to creade new deck.",
                    "OK");

                return;
            }
        }
    }
}

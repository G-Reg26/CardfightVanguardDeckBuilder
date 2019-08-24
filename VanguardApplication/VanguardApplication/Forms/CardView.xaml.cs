using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VanguardApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardView : ContentPage
    {
        private Deck.DeckType deckType;

        private Card card;

        private int maxAmountToAdd;
        private int deckLimit;

        public CardView(Card card, Deck currentDeck)
        {
            InitializeComponent();

            this.card = card;

            FillCardView();

            // fill deck picker with deck names
            DeckPicker.Items.Add("Add New Deck");

            foreach (Deck deck in App.Decks)
            {
                DeckPicker.Items.Add(deck.Name);
            }

            DeckPicker.SelectedIndexChanged += Deck_Changed;

            // set deck picker's selected index
            if (currentDeck == null)
            {
                DeckPicker.SelectedIndex = 0;
            }
            else
            {
                int currentDeckIndex = DeckPicker.Items.IndexOf(currentDeck.Name);
                DeckPicker.SelectedIndex = currentDeckIndex;
            }

            // determine deck limit and type through card type
            if (card.Type.Contains("G Unit"))
            {
                deckLimit = 16;
                deckType = Deck.DeckType.G;
            }
            else if (card.Type.Contains("Other"))
            {
                deckLimit = 0;
                deckType = Deck.DeckType.OTHER;
            }
            else
            {
                deckLimit = 50;
                deckType = Deck.DeckType.DECK;
            }
        }

        private void FillCardView()
        {
            Name.Text = card.Name;

            Set.Text = card.Set;

            CardImage.Source = card.Image;
            CardImage.Aspect = Aspect.AspectFit;

            Type.Text = card.Type;
            Group.Text = card.Group;
            Race.Text = card.Race;
            Nation.Text = card.Nation;
            Grade.Text = card.Grade;
            Power.Text = card.Power;
            Critical.Text = card.Critical;
            Shield.Text = card.Shield;
            Skill.Text = card.Skill;
            Gift.Text = card.Gift;

            Effect.Text = card.Effect;
            Flavor.Text = card.Flavor;

            Regulation.Text = card.Regulation;
            Number.Text = card.Number;
            Rarity.Text = card.Rarity;
            Illustrator.Text = card.Illustrator;
        }

        private void Deck_Changed(object sender, EventArgs e)
        {
            if (DeckPicker.SelectedIndex < 1)
            {
                NewDeckEntry.IsEnabled = true;
                NewDeckEntry.Text = "";
                maxAmountToAdd = deckType == Deck.DeckType.OTHER ? 0 : 4;
            }
            else
            {
                Deck deck = App.Decks.Find(r => r.Name == (string)DeckPicker.Items[DeckPicker.SelectedIndex]);

                NewDeckEntry.IsEnabled = false;
                NewDeckEntry.Text = Deck.MaxAmountToAdd(deck, card, deckType, deckLimit, ref maxAmountToAdd);
            }

            Amount2Add.Text = "" + maxAmountToAdd;
        }

        private void Add_2_Deck_Clicked(object sender, EventArgs e)
        {
            AddCardPopUp.IsVisible = true;
        }

        private void Exit_Button_Clicked(object sender, EventArgs e)
        {
            AddCardPopUp.IsVisible = false;
        }

        private void Left_Clicked(object sender, EventArgs e)
        {
            try
            {
                int amountToAdd = Int32.Parse(Amount2Add.Text);

                amountToAdd = amountToAdd > 0 ? amountToAdd - 1 : amountToAdd;

                Amount2Add.Text = "" + amountToAdd;
            }
            catch
            {
                Amount2Add.Text = "0";
            }
        }

        private void Right_Clicked(object sender, EventArgs e)
        {
            try
            {
                int amountToAdd = Int32.Parse(Amount2Add.Text);

                amountToAdd = (amountToAdd < maxAmountToAdd || deckType == Deck.DeckType.OTHER) ? amountToAdd + 1 : amountToAdd;

                Amount2Add.Text = "" + amountToAdd;
            }
            catch
            {
                Amount2Add.Text = "0";
            }
        }
        
        private void Add_Card_Clicked(object sender, EventArgs e)
        {
            Deck deck = null;

            // Create new deck option chosen
            if (DeckPicker.SelectedIndex == 0)
            {
                string header = "";
                string message = "";

                if (Deck.ValidDeckName(NewDeckEntry.Text, ref header, ref message))
                {
                    deck = new Deck() { Name = NewDeckEntry.Text };

                    App.Decks.Add(deck);
                    DeckPicker.Items.Add(deck.Name);
                }
                else
                {
                    DisplayAlert(header, message, "OK");
                    return;
                }
            }
            else
            {
                deck = App.Decks.Find(r => r.Name == (string)DeckPicker.Items[DeckPicker.SelectedIndex]);
            }

            try
            {
                Deck.AddToDeck(deckType, deck, Int32.Parse(Amount2Add.Text), deckLimit, card);
            }
            catch (Exception)
            {
                DisplayAlert("Failed to Save Deck", "System failed to save deck.", "OK");
            }

            if (DeckPicker.SelectedIndex == 0)
            {
                DeckPicker.SelectedIndex = DeckPicker.Items.Count - 1;
            }
            else
            {
                Deck_Changed(DeckPicker, new EventArgs());
            }
        }
    }
}
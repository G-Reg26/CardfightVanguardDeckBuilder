using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VanguardApplication
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeckView : ContentPage
    {
        private enum GridCellChildren
        {
            IMAGE_BUTTON,
            LABEL,
        };

        private enum State
        {
            VIEWING,
            EDITING
        }

        private const int DECK_COLUMNS = 5;

        readonly Deck deck;

        private State currentState;

        private bool cardSelected;

        public DeckView(Deck deck)
        {
            InitializeComponent();

            currentState = State.VIEWING;
            this.deck = deck;

            Title = this.deck.Name;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            UpdateGrid(deck.Cards, DeckGrid, DeckLabel);
            UpdateGrid(deck.G, GDeckGrid, GDeckLabel);
            UpdateGrid(deck.Others, OtherDeckGrid, OtherDeckLabel);
        }

        void UpdateGrid(List<Card> deck, Grid grid, Label label)
        {
            if (deck.Count == 0)
            {
                label.IsVisible = false;
                grid.IsVisible = false;
            }
            else
            {
                label.IsVisible = true;
                grid.IsVisible = true;

                // if cards were added to deck
                if (deck.Count != grid.Children.Count)
                {
                    // update grid cell content to match the sorted deck order
                    for (int i = 0; i < grid.Children.Count; i++)
                    {
                        StackLayout stackLayout = (StackLayout)grid.Children[i];
                        ImageButton imageButton = (ImageButton)stackLayout.Children[(int)GridCellChildren.IMAGE_BUTTON];
                        Label cardLabel = (Label)stackLayout.Children[(int)GridCellChildren.LABEL];

                        imageButton.Source = deck[i].Image;
                        cardLabel.Text = deck[i].Name;
                    }

                    // add new grid cells if needed
                    while (deck.Count != grid.Children.Count)
                    {
                        // add new row definition to grid if needed
                        if (grid.Children.Count % DECK_COLUMNS == 0)
                        {
                            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        }

                        int row = grid.Children.Count / DECK_COLUMNS;
                        int column = grid.Children.Count % DECK_COLUMNS;

                        grid.Children.Add(CreateGridContent(grid.Children.Count, deck), column, row);
                    }
                }
            }
        }

        public StackLayout CreateGridContent(int x, List<Card> deckAffected)
        {
            StackLayout stackLayout = new StackLayout();

            // create image button for card art
            ImageButton cardImage = new ImageButton()
            {
                Source = deckAffected[x].Image,
                HeightRequest = 100,
                ClassId = "c" + x,
            };

            cardImage.Clicked += Card_Clicked;

            // create card name label
            Label cardName = new Label()
            {
                Text = deckAffected[x].Name,
                TextColor = Color.Black,
                FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                HorizontalTextAlignment = TextAlignment.Center,
            };

            // add image button and label to stack layout
            stackLayout.Children.Add(cardImage);
            stackLayout.Children.Add(cardName);

            return stackLayout;
        }

        void Edit_Deck_Clicked(object sender, EventArgs e)
        {
            Edit.Text = Edit.Text == "Edit" ? "Done" : "Edit";

            currentState = currentState == State.VIEWING ? State.EDITING : State.VIEWING;
        }

        private async void Card_Clicked(object sender, EventArgs e)
        {
            if (!cardSelected)
            {
                cardSelected = true;

                Grid grid = (Grid)((ImageButton)sender).Parent.Parent;
                string imageButtonID = ((ImageButton)sender).ClassId;
                int index = Int32.Parse(imageButtonID.Substring(1));

                List<Card> cards;

                if (grid == GDeckGrid)
                {
                    cards = deck.G;
                }
                else if (grid == OtherDeckGrid)
                {
                    cards = deck.Others;
                }
                else
                {
                    cards = deck.Cards;
                }

                switch (currentState)
                {
                    case State.VIEWING:
                        await Navigation.PushAsync(new CardView(cards[index], deck));
                        break;
                    case State.EDITING:
                        RemoveCard(cards, index, grid);
                        break;
                }

                cardSelected = false;
            }
        }

        private async void RemoveCard(List<Card> cards, int index, Grid grid)
        {
            bool answer = await DisplayAlert(
                    "Remove Card?",
                    "Would you like to remove " + cards[index].Name + " from " + deck.Name + "?",
                    "Yes",
                    "No");

            if (answer)
            {
                cards.Remove(cards[index]);

                App.SaveDecks();

                for (int i = index; i < cards.Count; i++)
                {
                    StackLayout stackLayout = (StackLayout)grid.Children[i];
                    ImageButton imageButton = (ImageButton)stackLayout.Children[(int)GridCellChildren.IMAGE_BUTTON];
                    Label label = (Label)stackLayout.Children[(int)GridCellChildren.LABEL];

                    imageButton.Source = cards[i].Image;
                    label.Text = cards[i].Name;
                }

                if (grid.Children.Count > 0)
                {
                    grid.Children.Remove(grid.Children[grid.Children.Count - 1]);

                    if (grid.Children.Count % DECK_COLUMNS == 0)
                    {
                        grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);
                    }
                }
            }
        }

        private async void Add_Card_Clicked(object sender, EventArgs e)
        {
            AddCard.IsEnabled = false;

            await Navigation.PushAsync(new CardSearch(true, deck));

            AddCard.IsEnabled = true;
        }
    }
}
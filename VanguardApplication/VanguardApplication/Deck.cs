using System;
using System.Collections.Generic;
using System.Linq;

namespace VanguardApplication
{
    public class CardNameComparer : IComparer<Card>
    {
        public int Compare(Card x, Card y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return x.Name.CompareTo(y.Name);
                }
            }
        }
    }

    public class CardTypeComparer : IComparer<Card>
    {
        public int Compare(Card x, Card y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int t1 = x.Type.Contains("Trigger") ? 1 : 0;
                    int t2 = y.Type.Contains("Trigger") ? 1 : 0;

                    return t1.CompareTo(t2);
                }
            }
        }
    }

    [Serializable]
    public class Deck
    {
        public enum DeckType
        {
            DECK,
            G,
            OTHER,
        };

        public string Name { get; set; }
        public List<Card> Cards { get; set; }
        public List<Card> G { get; set; }
        public List<Card> Others { get; set; }

        public Deck()
        {
            Cards = new List<Card>();
            G = new List<Card>();
            Others = new List<Card>();
        }

        public int GetCardCount(Card card, DeckType deckType)
        {
            int count = 0;

            switch (deckType) {
                case DeckType.DECK:
                    foreach (Card c in Cards)
                    {
                        count = c.Equals(card) ? count + 1 : count;
                    }
                    break;
                case DeckType.G:
                    foreach (Card c in G)
                    {
                        count = c.Equals(card) ? count + 1 : count;
                    }
                    break;
                case DeckType.OTHER:
                    foreach (Card c in Others)
                    {
                        count = c.Equals(card) ? count + 1 : count;
                    }
                    break;
            }

            return count;
        }

        public void Sort(DeckType deckType)
        {
            switch (deckType)
            {
                case DeckType.DECK:
                    SortNormalDeck();
                    break;
                case DeckType.G:
                    G.Sort((a, b) => a.Name.CompareTo(b.Name));
                    break;
                case DeckType.OTHER:
                    Others.Sort((a, b) => a.Name.CompareTo(b.Name));
                    break;
            }
        }

        public void SortNormalDeck()
        {
            // sort deck by grade in descending order
            Cards.Sort(
                    delegate (Card c1, Card c2)
                    {
                        int g1 = Int32.Parse(c1.Grade.Substring(c1.Grade.Length - 1));
                        int g2 = Int32.Parse(c2.Grade.Substring(c2.Grade.Length - 1));

                        return g2.CompareTo(g1);
                    });

            // sort each grade in alphabetical order
            int begin = 0;
            int grade = 0;

            CardNameComparer cardNameComparer = new CardNameComparer();
            CardTypeComparer cardTypeComparer = new CardTypeComparer();

            for (int i = 0; i < Cards.Count; i++)
            {
                int currentGrade = Int32.Parse(Cards[i].Grade.Substring(Cards[i].Grade.Length - 1));
                if (currentGrade != grade)
                {
                    grade = currentGrade;

                    Cards.Sort(begin, i - begin, cardNameComparer);

                    begin = i;
                }
            }

            // sort grade 0s by their type (either Normal or Trigger)
            Cards.Sort(begin, Cards.Count - begin, cardTypeComparer);

            // sort Normal grade 0 units in alphabetical order
            for (int i = begin; i < Cards.Count; i++)
            {
                if (Cards[i].Type.Contains("Trigger"))
                {
                    Cards.Sort(begin, i - begin, cardNameComparer);

                    begin = i;

                    break;
                }
            }

            // sort Trigger units in alphabetical order
            Cards.Sort(begin, Cards.Count - begin, cardNameComparer);
        }

        public static bool ValidDeckName(string name, ref string header, ref string message)
        {
            // New deck entry is not an empty string
            if (name.Replace(" ", "") != "")
            {
                // Decks list does contains deck with new deck name
                if (App.Decks.Any(x => x.Name == name))
                {
                    header = "Duplicate Deck Name";
                    message = "Deck name is already in use. Please enter new deck name";

                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                header = "Enter Deck Name";
                message = "Please enter name to create new deck.";

                return false;
            }
        }

        public static void AddToDeck(DeckType deckType, Deck deck, int n, int deckLimit, Card card)
        {
            List<Card> cards;

            switch (deckType)
            {
                case Deck.DeckType.DECK:
                    cards = deck.Cards;
                    break;
                case Deck.DeckType.G:
                    cards = deck.G;
                    break;
                case Deck.DeckType.OTHER:
                    cards = deck.Others;
                    break;
                default:
                    cards = new List<Card>();
                    break;
            }

            for (int i = 0; i < n; i++)
            {
                if (cards.Count < deckLimit || deckType == Deck.DeckType.OTHER)
                {
                    cards.Add(new Card(card));
                }
                else
                {
                    break;
                }
            }

            deck.Sort(deckType);

            App.SaveDecks();
        }

        public static string MaxAmountToAdd(Deck deck, Card card, DeckType deckType, int deckLimit, ref int maxAmountToAdd)
        {
            int cardCount = deck.GetCardCount(card, deckType);

            if (deckType != Deck.DeckType.OTHER)
            {
                List<Card> deckAffected = deckType == Deck.DeckType.DECK ? deck.Cards : deck.G;

                if (deckAffected.Count == deckLimit)
                {
                    maxAmountToAdd = 0;
                    return (deckType == Deck.DeckType.DECK ? "Deck" : "G deck") + " is full!";
                }
                else
                {
                    maxAmountToAdd = 4 - cardCount;
                    return cardCount + "/" + 4 + " in deck.";
                }

            }
            else
            {
                maxAmountToAdd = 0;
                return cardCount + " in deck.";
            }
        }
    }
}

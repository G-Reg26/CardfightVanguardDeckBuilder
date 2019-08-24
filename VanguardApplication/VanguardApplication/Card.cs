using System;

namespace VanguardApplication
{
    [Serializable]
    public class Card
    {
        public string Name { get; set; }
        public string Effect { get; set; }
        public string Type { get; set; }
        public string Group { get; set; }
        public string Race { get; set; }
        public string Nation { get; set; }
        public string Grade { get; set; }
        public string Power { get; set; }
        public string Critical { get; set; }
        public string Shield { get; set; }
        public string Skill { get; set; }
        public string Gift { get; set; }
        public string Image { get; set; }
        public string Set { get; set; }
        public string Flavor { get; set; }
        public string Regulation { get; set; }
        public string Number { get; set; }
        public string Rarity { get; set; }
        public string Illustrator { get; set; }

        public Card() { }

        public Card(Card card)
        {
            Name = card.Name;
            Effect = card.Effect;
            Type = card.Type;
            Group = card.Group;
            Race = card.Race;
            Nation = card.Nation;
            Grade = card.Grade;
            Power = card.Power;
            Critical = card.Critical;
            Shield = card.Shield;
            Skill = card.Skill;
            Gift = card.Gift;
            Image = card.Image;
            Set = card.Set;
            Flavor = card.Flavor;
            Regulation = card.Regulation;
            Number = card.Number;
            Rarity = card.Rarity;
            Illustrator = card.Illustrator;
        }

        public bool Equals(Card card)
        {
            return Name.Equals(card.Name);
        }
    }
}

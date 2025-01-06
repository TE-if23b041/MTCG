using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGame.Enums;

namespace MonsterTradingCardsGame.Models
{
    // curl -i -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: Bearer admin-mtcgToken" -d "[
    // {\"Id\":\"d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8\",
    // \"Name\":\"WaterGoblin\",
    // \"Damage\":  9.0},

    // {\"Id\":\"44c82fbc-ef6d-44ab-8c7a-9fb19a0e7c6e\",
    // \"Name\":\"Dragon\",
    // \"Damage\": 55.0},

    // {\"Id\":\"2c98cd06-518b-464c-b911-8d787216cddd\",
    // \"Name\":\"WaterSpell\",
    // \"Damage\": 21.0},
    //
    // {\"Id\":\"951e886a-0fbf-425d-8df5-af2ee4830d85\",
    // \"Name\":\"Ork\",
    // \"Damage\": 55.0},
    //
    // {\"Id\":\"dcd93250-25a7-4dca-85da-cad2789f7198\",
    // \"Name\":\"FireSpell\",
    // \"Damage\": 23.0}]"


    public struct Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public MonsterType MonsterType { get; set; }
        public CardType CardType { get; set; }
        public ElementType ElementType { get; set; }

        // sets the type of the card based on the name
        public static MonsterType DetermineMonsterType(string name)
        {
            if(name.Contains("Goblin"))
                return MonsterType.Goblin;
            if (name.Contains("Dragon"))
                return MonsterType.Dragon;
            if (name.Contains("Wizard"))
                return MonsterType.Wizard;
            if (name.Contains("Ork"))
                return MonsterType.Ork;
            if (name.Contains("Knight"))
                return MonsterType.Knight;
            if (name.Contains("Kraken"))
                return MonsterType.Kraken;
            if (name.Contains("Elf"))
                return MonsterType.Elf;
            return MonsterType.None;
        }

        //  sets the type of the card based on the name
        public static CardType DetermineCardType(string name)
        {
            if (name.Contains("Spell"))
                return CardType.Spell;
            return CardType.Monster;
        }

        // sets the type of the card based on the name
        public static ElementType DetermineElementType(string name)
        {
            if (name.Contains("Water"))
                return ElementType.Water;
            if (name.Contains("Fire"))
                return ElementType.Fire;
            return ElementType.Normal;
        }
    }
}

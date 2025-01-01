using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Models
{
    public struct User //(int id, string username, string password, int coins, int elo)
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; }
        public int Elo { get; set; }
    }
}

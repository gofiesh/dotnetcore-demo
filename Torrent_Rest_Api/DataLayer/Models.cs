
using System.Collections.Generic;



namespace Torrent_WebApi.DataLayer
{
    public class BaseModel
    {
        public int Id { get; set; }
    }

    public class Game : BaseModel
    {
        public string Title { get; set; }
        public string ServerAddress { get; set; }

        public List<Team> Teams { get; set; }
    }

    public class Team : BaseModel
    {
        public int GameId { get; set; }

        public string Name { get; set; }

        public List<Player> Players { get; set; }
    }





    public class Account : BaseModel
    {
        public string First { get; set; }
        public string Last { get; set; }
        public string Email { get; set; }

        public List<Player> Players { get; set; }        
    }

    public class Player : BaseModel
    {
        public int TeamId { get; set; }
        public int AccountId { get; set; }

        public string GamerTag { get; set; }
        
        public Account Account { get; set; }
        public List<Token> Tokens { get; set; }
    }





    public class Token : BaseModel
    {
        public int PlayerId { get; set; }

        public string Name { get; set; }
        public string Class { get; set; }
        public string Identifier { get; set; }
    }
}


using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Torrent_WebApi.DataLayer;
using System.Collections.Generic;

/// <summary>
/// The Repository Pattern is implemented through RESTful API end points
/// via the Controllers below. Their JSON data definitions act as the
/// contract to the clients (browsers and game servers) and hides the
/// data storage implementation.
/// 
/// The implementation happens to use Entity Framework for .NET Core
/// to minimize our code, making it more maintainable, and help us
/// shape the data definition at the end points, but the data objects
/// never escape this web service.
/// 
/// It is intended that all game logic be found in game servers that
/// act as authorities for game progress.
/// </summary>
namespace Torrent_WebApi.Controllers
{

    [Route("api/team")]
    [Produces("application/json")]
    public class TeamController : DataController<Team> { public TeamController(RepoContext ctx) : base(ctx) { } }

    [Route("api/token")]
    [Produces("application/json")]
    public class TokenController : DataController<Token> { public TokenController(RepoContext ctx) : base(ctx) { } }

    [Route("api/player")]
    [Produces("application/json")]
    public class PlayerController : DataController<Player> { public PlayerController(RepoContext ctx) : base(ctx) { } }

    [Route("api/game")]
    [Produces("application/json")]
    public class GameController : DataController<Game>
    {
        public GameController(RepoContext ctx) : base(ctx) { this.ctx = ctx; }
        private RepoContext ctx = null;

        /// <summary>
        /// GET: api/game/deep
        /// 
        /// This method generates a JSON output representative of all
        /// games in play with detailed drill downs to the data object
        /// specified in the drill parameter.
        /// 
        /// The response includes an unlimited number of Game objects
        /// unless an error is reported in the response (400s, 500s).
        /// </summary>
        /// <param name="drill">The child object to drill down to.</param>
        /// <returns>A response with a JSON payload.</returns>
        [HttpGet("{drill}")]
        public IActionResult Get(string drill) { return Get(drill, 0); }

        /// <summary>
        /// GET: api/game/deep/max
        /// 
        /// This method generates a JSON output representative of all
        /// games in play with detailed drill downs to the data object
        /// specified in the drill parameter.
        /// 
        /// The response includes the first max Game objects (or less)
        /// unless an error is reported in the response (400s, 500s).
        /// </summary>
        /// <param name="drill">The child object to drill down to.</param>
        /// <param name="max">The max number of Game objects to return.</param>
        /// <returns>A response with a JSON payload.</returns>
        [HttpGet("{drill}/{max}")]
        public IActionResult Get(string drill, int max)
        {
            bool getToken = drill.ToLower() == "token";
            bool getAccount = drill.ToLower() == "account";
            bool getPlayer = getAccount || getToken || drill.ToLower() == "player";
            bool getTeam = getPlayer || drill.ToLower() == "team";

            var games = ctx.Set<Game>();
            var players = ctx.Set<Player>();
            var tokens = ctx.Set<Token>();
            var teams = ctx.Set<Team>();
            var accounts = ctx.Set<Account>();
            List<object> results = new List<object>();

            foreach (Game game in games)
            {
                if (--max == 0)
                    break;

                results.Add(new
                {
                    GameId = game.Id,
                    ServerIpAddress = game.ServerAddress,
                    GameTitle = game.Title,
                    Teams = teams
                        .Where(t => t.GameId == game.Id && getTeam)
                        .Select(t => new
                        {
                            TeamId = t.Id,
                            TeamName = t.Name,
                            TeamPlayers = players
                                .Where(p => p.TeamId == t.Id && getPlayer)
                                .Select(p => new
                                {
                                    PlayerId = p.Id,
                                    GamerTag = p.GamerTag,
                                    Account = accounts
                                        .Where(a => a.Id == p.AccountId && getAccount)
                                        .Select(a => new
                                        {
                                            AccountId = a.Id,
                                            FirstName = a.First,
                                            LastName = a.Last,
                                            Email = a.Email,
                                            GamerTags = players
                                                .Where(alias => alias.AccountId == a.Id)
                                                .Select(alias => alias.GamerTag)
                                                .ToList()
                                        }).SingleOrDefault(),
                                    Tokens = tokens
                                        .Where(tok => tok.PlayerId == p.Id & getToken)
                                        .Select(tok => new
                                        {
                                            TokenId = tok.Id,
                                            Name = tok.Name,
                                            Class = tok.Class,
                                            Identifier = tok.Identifier
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                });
            }
            // Return a deep list...
            return new ObjectResult(results);
        }
    }

    [Route("api/account")]
    [Produces("application/json")]
    public class AccountController : DataController<Account>
    {
        public AccountController(RepoContext ctx) : base(ctx) { this.ctx = ctx; }
        private RepoContext ctx = null;

        // GET: api/account/deep
        [HttpGet("{deep}")]
        public IActionResult Get(string deep)
        {
            var players = ctx.Set<Player>();
            var accounts = ctx.Set<Account>();
            var tokens = ctx.Set<Token>();
            List<object> results = new List<object>();

            foreach (Account account in accounts)
                results.Add(new
                {
                    First = account.First,
                    Last = account.Last,
                    Email = account.Email,

                    players = players
                        .Where(p => p.AccountId == account.Id)
                        .Select(p => new
                        {
                            PlayerId = p.Id,
                            GamerTag = p.GamerTag,
                            Tokens = tokens
                                        .Where(tok => tok.PlayerId == p.Id)
                                        .Select(tok => new
                                        {
                                            TokenId = tok.Id,
                                            Name = tok.Name,
                                            Class = tok.Class,
                                            Identifier = tok.Identifier
                                        }).ToList()
                        })
                        .ToList()
                });

            // Return a deep list...
            return new ObjectResult(results);
        }
    }


    public class DataController<T> : Controller where T : class
    {
        RepoContext Context;
        DbSet<T> DbSet;

        public DataController(RepoContext ctx)
        {
            Context = ctx;
            DbSet = ctx.Set<T>();
        }

        // GET: api/<controller>
        [HttpGet]
        virtual public IActionResult Get()
        {
            // Return a list of items, even if that list is empty...
            return new ObjectResult(DbSet.ToList<T>());
        }

        // GET: api/<controller>/5
        [HttpGet("{id}")]
        virtual public IActionResult Get(int id)
        {
            // If the item doesn't exist...
            T item = DbSet.Find(id);
            if (item == null)
                return NotFound();

            // Return the item...
            return new ObjectResult(item);
        }

        // POST: api/<controller>
        [HttpPost]
        virtual public IActionResult Post([FromBody]T value)
        {
            // Update the db with the new item added...
            DbSet.Add(value);
            Context.SaveChanges();
            return Ok();
        }

        // PUT: api/<controller>/5
        /// <summary>
        /// This PUT action will update an existing ORM object. It will not
        /// create a new object to satisfy the given URI. This is within
        /// HTTP 1.1 specification, which states:
        /// 
        /// <code>
        /// The PUT method requests that the enclosed entity be stored under 
        /// the supplied Request-URI. If the Request-URI refers to an already
        /// existing resource, the enclosed entity SHOULD be considered as a 
        /// modified version of the one residing on the origin server. If the 
        /// Request-URI does not point to an existing resource, and that URI 
        /// is capable of being defined as a new resource by the requesting 
        /// user agent, the origin server can create the resource with that URI.
        /// </code>
        /// 
        /// This limitation is due to the primary key being under full control
        /// of the ORM and therefore out of the reach of the client. 
        /// </summary>
        /// <param name="id">The primary key of the ORM object.</param>
        /// <param name="value">The object to use in updating the ORM.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        virtual public IActionResult Put(int id, [FromBody]T value)
        {
            // if the request is bad...
            if (value == null || (value as BaseModel).Id != id)
                return BadRequest();

            // If the item doesn't exist...
            T item = DbSet.Find(id);
            if (item == null)
                return NotFound();
            
            // Swap out the new item for the existing one...
            Context.Entry(item).State = EntityState.Detached;
            DbSet.Attach(value);

            // Update the db with the new item...
            DbSet.Update(value);
            Context.SaveChanges();
            return Ok();
        }

        // DELETE: api/<controller>/5
        [HttpDelete("{id}")]
        virtual public IActionResult Delete(int id)
        {
            // if the item doesn't exist...
            T o = DbSet.Find(id);
            if (o == null)
                return NotFound();

            // Update the db with the new item removed...
            DbSet.Remove(o);
            Context.SaveChanges();
            return Ok();

        }
    }
}

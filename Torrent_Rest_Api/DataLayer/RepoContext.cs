using System;
using Microsoft.EntityFrameworkCore;



/// <summary>
/// To generate an initial migration and create the underlining database, perform
/// the following commands:
/// 
///         > dotnet ef migrations add init
/// 
///         > dotnet ef database update
/// 
/// </summary>
namespace Torrent_WebApi.DataLayer
{
    public class RepoContext : DbContext
    {
        public RepoContext(DbContextOptions<RepoContext> options) : base(options) { }

        public DbSet<Game> Game { get; set; }
        public DbSet<Team> Team { get; set; }
        public DbSet<Token> Token { get; set; }
        public DbSet<Player> Player { get; set; }
        public DbSet<Account> Account { get; set; }

        public void Commit()
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WebApiMock.Data {
    public class DataContext : DbContext {

        public DataContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            var conStrBld = new SqliteConnectionStringBuilder {
                DataSource = "mock-data.db",
                Mode = SqliteOpenMode.ReadWriteCreate
            };
            optionsBuilder.UseSqlite(conStrBld.ConnectionString);
        }


        public DbSet<ResponseDefinition> Responses { get; set; }

        public DbSet<RequestDefinition> Requests { get; set; }
    }
}

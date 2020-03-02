using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WebApiMock.Data {

    /// <inheritdoc/>
    public class DataContext : DbContext {

        /// <inheritdoc/>
        public DataContext() => Database.EnsureCreated();

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            var conStrBld = new SqliteConnectionStringBuilder {
                DataSource = "mock-data.db",
                Mode = SqliteOpenMode.ReadWriteCreate
            };
            optionsBuilder.UseSqlite(conStrBld.ConnectionString);
        }

        /// <summary>
        /// Mock-up response definitions.
        /// </summary>
        /// <remarks>
        /// Response definitions are referenced by request definitions.
        /// </remarks>
        public DbSet<ResponseDefinition> Responses { get; set; }

        /// <summary>
        /// Mock-up request definitions.
        /// </summary>
        public DbSet<RequestDefinition> Requests { get; set; }
    }
}

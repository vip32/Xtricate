using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;

namespace Xtricate.DocSet.Sqlite
{
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public DbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }
    }
}

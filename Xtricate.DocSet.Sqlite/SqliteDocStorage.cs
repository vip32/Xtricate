using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xtricate.DocSet.Sqlite
{
    public class SqliteDocStorage<TDoc> : DocStorage<TDoc>
    {
        public SqliteDocStorage(IDbConnectionFactory connectionFactory, IStorageOptions options, ISqlBuilder sqlBuilder,
            ISerializer serializer, IHasher hasher = null, IEnumerable<IIndexMap<TDoc>> indexMaps = null)
            : base(connectionFactory, options, sqlBuilder, serializer, hasher, indexMaps)
        {
        }

        protected override void EnsureSchema(IStorageOptions options)
        {
            return; // not needed
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper
{
    public static class DapperExtensions
    {
        public static async Task<T> InsertAsync<T>(this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<T> result = await SqlMapper.QueryAsync<T>(cnn, sql, param, transaction, commandTimeout, commandType);
            return result.Single();
        }
    }
}
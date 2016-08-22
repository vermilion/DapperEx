using System.Collections.Generic;
using Dapper;

namespace Xerox.Archive.AdoLib.DapperHelpers
{
    public class MapChain<T>
    {
        public MapChain(SqlMapper.GridReader reader, IEnumerable<T> data)
            : this(data)
        {
            Reader = reader;
        }

        public MapChain(IEnumerable<T> data)
        {
            Data = data;
        }

        public SqlMapper.GridReader Reader { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
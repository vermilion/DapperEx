using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Xerox.Archive.AdoLib.DapperHelpers
{
    public static class MapHelpers
    {
        public static MapChain<TFirst> StartMap<TFirst>(this SqlMapper.GridReader reader, DataMapper<TFirst> mapper) where TFirst : class
        {
            var first = reader.Read(mapper);
            return new MapChain<TFirst>(reader, first);
        }

        public static MapChain<TFirst> NextMultiple<TFirst, TSecond, TKey>(this MapChain<TFirst> chain, DataMapper<TSecond> mapper, Func<TFirst, TKey> firstKey, Func<TSecond, TKey> secondKey, Action<TFirst, IEnumerable<TSecond>> addChildren)
        {
            var childMap = chain.Reader
                .Read(mapper)
                .GroupBy(secondKey)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (TFirst item in chain.Data)
            {
                IEnumerable<TSecond> children;
                if (childMap.TryGetValue(firstKey(item), out children))
                {
                    addChildren(item, children);
                }
            }

            return chain;
        }

        public static MapChain<TFirst> NextSingle<TFirst, TSecond, TKey>(this MapChain<TFirst> chain, DataMapper<TSecond> mapper, Func<TFirst, TKey> firstKey, Func<TSecond, TKey> secondKey, Action<TFirst, TSecond> addChild)
        {
            var childMap = chain.Reader
                .Read(mapper)
                .GroupBy(secondKey)
                .ToDictionary(g => g.Key, g => g.Single());

            foreach (var item in chain.Data)
            {
                TSecond child;
                if (childMap.TryGetValue(firstKey(item), out child))
                {
                    addChild(item, child);
                }
            }

            return chain;
        }

        public static IEnumerable<TFirst> EndMap<TFirst>(this MapChain<TFirst> chain)
        {
            return chain.Data;
        }




        /// <summary>
        ///     Read the next grid of results
        /// </summary>
        public static IEnumerable<T> Read<T>(this SqlMapper.GridReader reader, DataMapper<T> mapper, bool buffered = true)
        {
            return reader.ReadImpl(mapper, buffered);
        }

        private static IEnumerable<T> ReadImpl<T>(this SqlMapper.GridReader reader, DataMapper<T> mapper, bool buffered)
        {
            IEnumerable<T> result = reader.ReadDeferred(reader.GridIndex, mapper);
            return buffered ? result.ToList() : result;
        }

        private static IEnumerable<T> ReadDeferred<T>(this SqlMapper.GridReader gridReader, int index, DataMapper<T> mapper)
        {
            try
            {
                while (index == gridReader.GridIndex && gridReader.Reader.Read())
                {
                    mapper.Actions.Reverse();

                    int currentPos = gridReader.Reader.FieldCount;
                    foreach (var data in mapper.Actions)
                    {
                        var deserializer = SqlMapper.GetDeserializer(data.Type, data.SplitOn, gridReader.Reader, ref currentPos);
                        data.Result = deserializer(gridReader.Reader);
                    }

                    mapper.Actions.Reverse();
                    yield return mapper.Actions.Aggregate(default(T), (current, data) => data.Process(current));
                }
            }
            finally // finally so that First etc progresses things even when multiple rows
            {
                if (index == gridReader.GridIndex)
                {
                    gridReader.NextResult();
                }
            }
        }
    }
}
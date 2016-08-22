using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Xerox.Archive.AdoLib.DapperHelpers
{
    public class DataMapper<TFirst>
    {
        public readonly List<MapperData> Actions = new List<MapperData>();

        public DataMapper(Expression<Func<TFirst, object>> splitProp)
        {
            Actions.Add(new MapperDataGeneric<TFirst>
            {
                Type = typeof(TFirst),
                SplitOn = GetMemberName(splitProp),
                Action = (x, y) => y
            });
        }

        public DataMapper<TFirst> OneToOne<T>(Action<TFirst, T> action, Expression<Func<T, object>> splitProp)
        {
            Actions.Add(new MapperDataGeneric<T>
            {
                Type = typeof(T),
                SplitOn = GetMemberName(splitProp),
                Action = (x, y) =>
                {
                    action(x, y);
                    return x;
                }
            });

            return this;
        }

        private string GetMemberName<T>(Expression<Func<T, object>> splitProp)
        {
            var expression = (MemberExpression)((UnaryExpression)splitProp.Body).Operand;
            return expression.Member.Name;
        }

        public abstract class MapperData
        {
            public Type Type { get; set; }
            public string SplitOn { get; set; }
            public object Result { get; set; }
            public abstract TFirst Process(TFirst first);
        }

        public class MapperDataGeneric<T> : MapperData
        {
            public Func<TFirst, T, TFirst> Action { get; set; }

            public override TFirst Process(TFirst first)
            {
                var result = (T)Result;
                return Action(first, result);
            }
        }
    }
}
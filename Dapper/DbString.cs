using System;
using System.Data;

namespace Dapper
{
    /// <summary>
    /// This class represents a SQL string, it can be used if you need to denote your parameter is a Char vs VarChar vs nVarChar vs nChar
    /// </summary>
    sealed partial class DbString : Dapper.SqlMapper.ICustomQueryParameter
    {
        /// <summary>
        /// A value to set the default value of strings
        /// going through Dapper. Default is 4000, any value larger than this
        /// field will not have the default value applied.
        /// </summary>
        public const int DefaultLength = 4000;

        /// <summary>
        /// Create a new DbString
        /// </summary>
        public DbString() { Length = -1; }
        /// <summary>
        /// Ansi vs Unicode 
        /// </summary>
        public bool IsAnsi { get; set; }
        /// <summary>
        /// Fixed length 
        /// </summary>
        public bool IsFixedLength { get; set; }
        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// The value of the string
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Add the parameter to the command... internal use only
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddParameter(IDbCommand command, string name)
        {
            if (IsFixedLength && Length == -1)
            {
                throw new InvalidOperationException("If specifying IsFixedLength,  a Length must also be specified");
            }
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = SqlMapper.SanitizeParameterValue(Value);
            if (Length == -1 && Value != null && Value.Length <= DefaultLength)
            {
                param.Size = DefaultLength;
            }
            else
            {
                param.Size = Length;
            }
            param.DbType = IsAnsi ? (IsFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString) : (IsFixedLength ? DbType.StringFixedLength : DbType.String);
            command.Parameters.Add(param);
        }
    }
}
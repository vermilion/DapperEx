using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Dapper
{
    /// <summary>
    ///     Represents the key aspects of a sql operation
    /// </summary>
    public struct CommandDefinition
    {
        private static SqlMapper.Link<Type, Action<IDbCommand>> _commandInitCache;
        private readonly CancellationToken _cancellationToken;
        private readonly string _commandText;
        private readonly int? _commandTimeout;
        private readonly CommandType? _commandType;
        private readonly CommandFlags _flags;
        private readonly object _parameters;
        private readonly IDbTransaction _transaction;

        /// <summary>
        ///     Initialize the command definition
        /// </summary>
        public CommandDefinition(string commandText, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered
            , CancellationToken cancellationToken = default(CancellationToken)
            )
        {
            _commandText = commandText;
            _parameters = parameters;
            _transaction = transaction;
            _commandTimeout = commandTimeout;
            _commandType = commandType;
            _flags = flags;
            _cancellationToken = cancellationToken;
        }

        private CommandDefinition(object parameters)
            : this()
        {
            _parameters = parameters;
        }


        /// <summary>
        ///     The command (sql or a stored-procedure name) to execute
        /// </summary>
        public string CommandText
        {
            get { return _commandText; }
        }

        /// <summary>
        ///     The parameters associated with the command
        /// </summary>
        public object Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        ///     The active transaction for the command
        /// </summary>
        public IDbTransaction Transaction
        {
            get { return _transaction; }
        }

        /// <summary>
        ///     The effective timeout for the command
        /// </summary>
        public int? CommandTimeout
        {
            get { return _commandTimeout; }
        }

        /// <summary>
        ///     The type of command that the command-text represents
        /// </summary>
        public CommandType? CommandType
        {
            get { return _commandType; }
        }

        /// <summary>
        ///     Should data be buffered before returning?
        /// </summary>
        public bool Buffered
        {
            get { return (_flags & CommandFlags.Buffered) != 0; }
        }

        /// <summary>
        ///     Should the plan for this query be cached?
        /// </summary>
        internal bool AddToCache
        {
            get { return (_flags & CommandFlags.NoCache) == 0; }
        }

        /// <summary>
        ///     Additional state flags against this command
        /// </summary>
        public CommandFlags Flags
        {
            get { return _flags; }
        }

        /// <summary>
        ///     Can async queries be pipelined?
        /// </summary>
        public bool Pipelined
        {
            get { return (_flags & CommandFlags.Pipelined) != 0; }
        }

        /// <summary>
        ///     For asynchronous operations, the cancellation-token
        /// </summary>
        public CancellationToken CancellationToken
        {
            get { return _cancellationToken; }
        }

        internal static CommandDefinition ForCallback(object parameters)
        {
            if (parameters is DynamicParameters)
            {
                return new CommandDefinition(parameters);
            }
            return default(CommandDefinition);
        }

        internal void OnCompleted()
        {
            if (_parameters is SqlMapper.IParameterCallbacks)
            {
                ((SqlMapper.IParameterCallbacks) _parameters).OnCompleted();
            }
        }

        internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader)
        {
            IDbCommand cmd = cnn.CreateCommand();
            Action<IDbCommand> init = GetInit(cmd.GetType());
            if (init != null) init(cmd);
            if (_transaction != null)
                cmd.Transaction = _transaction;
            cmd.CommandText = _commandText;
            if (_commandTimeout.HasValue)
                cmd.CommandTimeout = _commandTimeout.Value;
            if (_commandType.HasValue)
                cmd.CommandType = _commandType.Value;
            if (paramReader != null)
            {
                paramReader(cmd, _parameters);
            }
            return cmd;
        }

        private static Action<IDbCommand> GetInit(Type commandType)
        {
            if (commandType == null) return null; // GIGO
            Action<IDbCommand> action;
            if (SqlMapper.Link<Type, Action<IDbCommand>>.TryGet(_commandInitCache, commandType, out action))
            {
                return action;
            }
            MethodInfo bindByName = GetBasicPropertySetter(commandType, "BindByName", typeof (bool));
            MethodInfo initialLongFetchSize = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof (int));

            action = null;
            if (bindByName != null || initialLongFetchSize != null)
            {
                var method = new DynamicMethod(commandType.Name + "_init", null, new[] {typeof (IDbCommand)});
                ILGenerator il = method.GetILGenerator();

                if (bindByName != null)
                {
                    // .BindByName = true
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.EmitCall(OpCodes.Callvirt, bindByName, null);
                }
                if (initialLongFetchSize != null)
                {
                    // .InitialLONGFetchSize = -1
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.EmitCall(OpCodes.Callvirt, initialLongFetchSize, null);
                }
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand>) method.CreateDelegate(typeof (Action<IDbCommand>));
            }
            // cache it            
            SqlMapper.Link<Type, Action<IDbCommand>>.TryAdd(ref _commandInitCache, commandType, ref action);
            return action;
        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
        {
            PropertyInfo prop = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            ParameterInfo[] indexers;
            if (prop != null && prop.CanWrite && prop.PropertyType == expectedType
                && ((indexers = prop.GetIndexParameters()) == null || indexers.Length == 0))
            {
                return prop.GetSetMethod();
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper
{
    [AssemblyNeutral, AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class AssemblyNeutralAttribute : Attribute
    {
    }

    public partial class SqlMapper
    {
    }

    public partial class DynamicParameters
    {
    }

    public partial class DbString
    {
    }


    public partial class SimpleMemberMap
    {
    }

    public partial class DefaultTypeMap
    {
    }

    public partial class CustomPropertyTypeMap
    {
    }

    public partial class FeatureSupport
    {
    }
}
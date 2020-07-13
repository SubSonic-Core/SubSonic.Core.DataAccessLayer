using System;

namespace SubSonic.Extensions.Test.Data.Functions
{
    public static class Scalar
    {
        [DbScalarFunction(nameof(IsPropertyAvailable))]
        public static bool IsPropertyAvailable(int statusId)
        {
            throw new NotImplementedException(SubSonicErrorMessages.SqlMethodOnlyForSql.Format(nameof(IsPropertyAvailable)));
        }

        [DbScalarFunction(nameof(SupportsMultipleArguments))]
        public static bool SupportsMultipleArguments(int statusId, bool isUnderDevelopment)
        {
            throw new NotImplementedException(SubSonicErrorMessages.SqlMethodOnlyForSql.Format(nameof(SupportsMultipleArguments)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Extensions.Test.Data.Functions
{
    using Infrastructure;

    public static class Scalar
    {
        [DbScalarFunction(nameof(IsPropertyAvailable))]
        public static bool IsPropertyAvailable(int statusId)
        {
            throw new NotImplementedException(SubSonicErrorMessages.SqlMethodOnlyForSql.Format(nameof(IsPropertyAvailable)));
        }
    }
}

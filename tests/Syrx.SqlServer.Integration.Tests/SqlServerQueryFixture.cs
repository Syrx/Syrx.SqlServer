//  ============================================================================================================================= 
//  author       : david sexton (@sextondjc | sextondjc.com)
//  date         : 2017.09.29 (21:39)
//  modified     : 2017.10.01 (20:41)
//  licence      : This file is subject to the terms and conditions defined in file 'LICENSE.txt', which is part of this source code package.
//  =============================================================================================================================

using Syrx.Commanders.Databases.Integration.Tests;
using Syrx.SqlServer.Integration.Tests.Setup;
using Syrx.Tests.Common;

namespace Syrx.SqlServer.Integration.Tests
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SqlServerQueryFixture : QueryFixture
    {
        public SqlServerQueryFixture() : base(CommanderHelper.UseSqlServer<Query>())
        {
            var setup = new DatabaseSetup();
            setup.Setup();
        }
    }
}
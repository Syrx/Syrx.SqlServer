namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    public static class SqlServerSetupExtensions
    {
        public static SyrxBuilder SetupSqlServer(this SyrxBuilder builder, string alias, string connectionString)
        {
            return builder.UseSqlServer(
                            b => b
                            .AddConnectionString(alias, connectionString)
                            //.AddDatabaseSetup(alias)
                            .AddSetupBuilderOptions(alias)
                            .AddQueryMultimap(alias)
                            .AddQueryMultiple(alias)
                            .AddExecute(alias)
                            //.AddDisposeCommands()
                            );
        }
                
        public static CommanderSettingsBuilder AddSetupBuilderOptions(this CommanderSettingsBuilder builder, string alias)
        {
            return builder.AddCommand(
                a => a.ForType<DatabaseBuilder>(
                    b => b
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateDatabase), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateDatabase))
                    .ForMethod(
                        nameof(DatabaseBuilder.DropTableCreatorProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.DropTableCreatorProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateTableCreatorProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateTableCreatorProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateTable), c => c
                        .UseConnectionAlias(alias)
                        .SetCommandType(CommandType.StoredProcedure)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateTable))
                    .ForMethod(
                        nameof(DatabaseBuilder.DropIdentityTesterProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.DropIdentityTesterProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateIdentityTesterProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateIdentityTesterProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.DropBulkInsertProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.DropBulkInsertProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateBulkInsertProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateBulkInsertProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.DropBulkInsertAndReturnProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.DropBulkInsertAndReturnProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateBulkInsertAndReturnProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateBulkInsertAndReturnProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.DropTableClearingProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.DropTableClearingProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.CreateTableClearingProcedure), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.CreateTableClearingProcedure))
                    .ForMethod(
                        nameof(DatabaseBuilder.ClearTable), c => c
                        .UseConnectionAlias(alias)
                        .SetCommandType(CommandType.StoredProcedure)
                        .UseCommandText(SqlServerCommandStrings.Setup.ClearTable))
                    .ForMethod(
                        nameof(DatabaseBuilder.Populate), c => c
                        .UseConnectionAlias(alias)
                        .UseCommandText(SqlServerCommandStrings.Setup.Populate))
                    ));
        }
                        
        public static CommanderSettingsBuilder AddQueryMultimap(this CommanderSettingsBuilder builder, string alias)
        {
            return builder.AddCommand(
                    b => b.ForType<Query>(
                        c => c
                        .ForMethod(
                            nameof(Query.ExceptionsAreReturnedToCaller), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.ExceptionsAreReturnedToCaller))
                        .ForMethod(
                            nameof(Query.SingleType), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.SingleType))
                        .ForMethod(
                            nameof(Query.SingleTypeWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.SingleTypeWithParameters))
                        .ForMethod(
                            nameof(Query.TwoTypes), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.TwoTypes))
                        .ForMethod(
                            nameof(Query.TwoTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.TwoTypesWithParameters))
                        .ForMethod(
                            nameof(Query.ThreeTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.ThreeTypesWithParameters))
                        .ForMethod(
                            nameof(Query.FourTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.FourTypesWithParameters))
                        .ForMethod(
                            nameof(Query.FiveTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.FiveTypesWithParameters))
                        .ForMethod(
                            nameof(Query.SixTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.SixTypesWithParameters))
                        .ForMethod(
                            nameof(Query.SevenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.SevenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.EightTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.EightTypesWithParameters))
                        .ForMethod(
                            nameof(Query.NineTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.NineTypesWithParameters))
                        .ForMethod(
                            nameof(Query.TenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.TenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.ElevenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.ElevenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.TwelveTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.TwelveTypesWithParameters))
                        .ForMethod(
                            nameof(Query.ThirteenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.ThirteenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.FourteenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.FourteenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.FifteenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.FifteenTypesWithParameters))
                        .ForMethod(
                            nameof(Query.SixteenTypesWithParameters), d => d
                            .UseConnectionAlias(alias)
                            .UseCommandText(SqlServerCommandStrings.Query.Multimap.SixteenTypesWithParameters))));

        }       

        public static CommanderSettingsBuilder AddQueryMultiple(this CommanderSettingsBuilder builder, string alias)
        {
            return builder.AddCommand(
                b => b.ForType<Query>(c => c
                .ForMethod(
                    nameof(Query.OneTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.OneTypeMultiple))
                .ForMethod(
                    nameof(Query.TwoTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.TwoTypeMultiple))
                .ForMethod(
                    nameof(Query.ThreeTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.ThreeTypeMultiple))
                .ForMethod(
                    nameof(Query.FourTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.FourTypeMultiple))
                .ForMethod(
                    nameof(Query.FiveTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.FiveTypeMultiple))
                .ForMethod(
                    nameof(Query.SixTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.SixTypeMultiple))
                .ForMethod(
                    nameof(Query.SevenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.SevenTypeMultiple))
                .ForMethod(
                    nameof(Query.EightTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.EightTypeMultiple))
                .ForMethod(
                    nameof(Query.NineTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.NineTypeMultiple))
                .ForMethod(
                    nameof(Query.TenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.TenTypeMultiple))
                .ForMethod(
                    nameof(Query.ElevenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.ElevenTypeMultiple))
                .ForMethod(
                    nameof(Query.TwelveTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.TwelveTypeMultiple))
                .ForMethod(
                    nameof(Query.ThirteenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.ThirteenTypeMultiple))
                .ForMethod(
                    nameof(Query.FourteenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.FourteenTypeMultiple))
                .ForMethod(
                    nameof(Query.FifteenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.FifteenTypeMultiple))
                .ForMethod(
                    nameof(Query.SixteenTypeMultiple), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Query.Multiple.SixteenTypeMultiple))));
        }

        public static CommanderSettingsBuilder AddExecute(this CommanderSettingsBuilder builder, string alias)
        {
            return builder.AddCommand(
                b => b.ForType<Execute>(c => c
                .ForMethod(
                    nameof(Execute.ExceptionsAreReturnedToCaller), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.ExceptionsAreReturnedToCaller))                
                .ForMethod(
                    nameof(Execute.SupportParameterlessCalls), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportParameterlessCalls))                
                .ForMethod(
                    $"{nameof(Execute.SupportsRollbackOnParameterlessCalls)}.Count", d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsRollbackOnParameterlessCallsCount))
                .ForMethod(
                    nameof(Execute.SupportsRollbackOnParameterlessCalls), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsRollbackOnParameterlessCalls))
                .ForMethod(
                    nameof(Execute.SupportsSuppressedDistributedTransactions), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsSuppressedDistributedTransactions))
                .ForMethod(
                    $"{nameof(Execute.SupportsTransactionRollback)}.Count", d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsTransactionRollbackCount))
                .ForMethod(
                    nameof(Execute.SupportsTransactionRollback), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsTransactionRollback))
                .ForMethod(
                    nameof(Execute.SupportsEnlistingInAmbientTransactions), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SupportsEnlistingInAmbientTransactions))
                .ForMethod(
                    nameof(Execute.SuccessfullyWithResponse), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SuccessfullyWithResponse))
                .ForMethod(
                    $"{nameof(Execute.SuccessfullyWithResponse)}.Response", d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SuccessfullyWithResponseResponse))
                .ForMethod(
                    nameof(Execute.Successful), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.Successful))
                .ForMethod(
                    nameof(Execute.SingleType), d => d
                    .UseConnectionAlias(alias)
                    .UseCommandText(SqlServerCommandStrings.Execute.SingleType))
                
                ));
        }

        /*
        public static CommanderSettingsBuilder AddDisposeCommands(this CommanderSettingsBuilder builder)
        {
            return builder.AddCommand(
                a => a.ForType<Dispose>(b => b
                    .ForMethod(
                        nameof(Dispose.Successfully), c => c
                        .UseConnectionAlias(SqlServerCommandStrings.Alias)
                        .UseCommandText(SqlServerCommandStrings.Dispose.Successfully))));
        }
        */
    }


}

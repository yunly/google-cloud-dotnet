// Copyright 2017 Google Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Cloud.ClientTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Xunit;
using Google.Cloud.Spanner.Data.CommonTesting;

#if !NETCOREAPP1_0
using System.Transactions;
#endif

// All samples now use a connection string variable declared before the start of the snippet.
// There are pros and cons for this:
// - Pro: The tests still run correctly when the fixture specifies extra configuration, e.g. credentials or host
// - Pro: The code is shorter (as connection string building can be verbose, particularly when already indented)
// - Con: There are fewer examples of building a connection string
// - Unsure: Arguably a connection string should be built elsewhere and reused, rather than appearing in the
//           code that creates a SpannerConnection. We need to see what actual usage tends towards.
//
// The table name of "TestTable" is hard-coded here, rather than taken from _fixture.TableName. This probably
// leads to simpler snippets.

namespace Google.Cloud.Spanner.Data.Snippets
{
    [SnippetOutputCollector]
    [Collection(nameof(SampleTableFixture))]
    [FileLoggerBeforeAfterTest]
    public class SpannerConnectionSnippets
    {
        private readonly SampleTableFixture _fixture;

        public SpannerConnectionSnippets(SampleTableFixture fixture) => _fixture = fixture;

        [Fact]
        public void CreateConnection()
        {
            // Snippet: #ctor(string, ChannelCredentials)
            string connectionString = "Data Source=projects/my-project/instances/my-instance/databases/my-db";
            SpannerConnection connection = new SpannerConnection(connectionString);
            Console.WriteLine(connection.Project);
            Console.WriteLine(connection.SpannerInstance);
            Console.WriteLine(connection.Database);
            // End snippet

            Assert.Equal("my-project", connection.Project);
            Assert.Equal("my-instance", connection.SpannerInstance);
            Assert.Equal("my-db", connection.Database);
        }

        [Fact]
        public async Task CreateDatabaseAsync()
        {
            string databaseName = $"{_fixture.Database.SpannerDatabase}_extra";
            string connectionString = new SpannerConnectionStringBuilder(_fixture.ConnectionString)
                .WithDatabase(databaseName)
                .ConnectionString;

            // Sample: CreateDatabaseAsync
            // Additional: CreateDdlCommand
            using (SpannerConnection connection = new SpannerConnection(connectionString))
            {
                SpannerCommand createDbCmd = connection.CreateDdlCommand($"CREATE DATABASE {databaseName}");
                await createDbCmd.ExecuteNonQueryAsync();

                SpannerCommand createTableCmd = connection.CreateDdlCommand(
                    @"CREATE TABLE TestTable (
                                            Key                STRING(MAX) NOT NULL,
                                            StringValue        STRING(MAX),
                                            Int64Value         INT64,
                                          ) PRIMARY KEY (Key)");
                await createTableCmd.ExecuteNonQueryAsync();
            }
            // End sample

            using (SpannerConnection connection = new SpannerConnection(connectionString))
            {
                SpannerCommand createDbCmd = connection.CreateDdlCommand($"DROP DATABASE {databaseName}");
                await createDbCmd.ExecuteNonQueryAsync();
            }
        }

        [Fact]
        public async Task InsertDataAsync()
        {
            string connectionString = _fixture.ConnectionString;

            await RetryHelpers.RetryOnceAsync(async () =>
            {
                // Sample: InsertDataAsync
                using (SpannerConnection connection = new SpannerConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SpannerCommand cmd = connection.CreateInsertCommand("TestTable");
                    SpannerParameter keyParameter = cmd.Parameters.Add("Key", SpannerDbType.String);
                    SpannerParameter stringValueParameter = cmd.Parameters.Add("StringValue", SpannerDbType.String);
                    SpannerParameter int64ValueParameter = cmd.Parameters.Add("Int64Value", SpannerDbType.Int64);

                    // This executes 5 distinct transactions with one row written per transaction.
                    for (int i = 0; i < 5; i++)
                    {
                        keyParameter.Value = Guid.NewGuid().ToString("N");
                        stringValueParameter.Value = $"StringValue{i}";
                        int64ValueParameter.Value = i;
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine($"{rowsAffected} rows written...");
                    }
                }
                // End sample
            });
        }        

        [Fact]
        public async Task CommitTimestampAsync()
        {
            string connectionString = _fixture.ConnectionString;

            await RetryHelpers.RetryOnceAsync(async () =>
            {
                // Sample: CommitTimestamp
                using (SpannerConnection connection = new SpannerConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string createTableStatement =
                        @"CREATE TABLE UsersHistory (
                        Id INT64 NOT NULL,
                        CommitTs TIMESTAMP NOT NULL OPTIONS
                            (allow_commit_timestamp=true),
                        Name STRING(MAX) NOT NULL,
                        Email STRING(MAX),
                        Deleted BOOL NOT NULL,
                      ) PRIMARY KEY(Id, CommitTs DESC)";

                    await connection.CreateDdlCommand(createTableStatement).ExecuteNonQueryAsync();

                    // Insert a first row
                    SpannerCommand cmd = connection.CreateInsertCommand("UsersHistory",
                        new SpannerParameterCollection
                        {
                            { "Id", SpannerDbType.Int64, 10L },
                            { "CommitTs", SpannerDbType.Timestamp, SpannerParameter.CommitTimestamp },
                            { "Name", SpannerDbType.String, "Demo 1" },
                            { "Deleted", SpannerDbType.Bool, false }
                        });
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    // Insert a second row
                    cmd.Parameters["Id"].Value = 11L;
                    cmd.Parameters["Name"].Value = "Demo 2";
                    rowsAffected += await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"{rowsAffected} rows written...");

                    // Display the inserted values
                    SpannerCommand selectCmd = connection.CreateSelectCommand("SELECT * FROM UsersHistory");
                    using (SpannerDataReader reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            long id = reader.GetFieldValue<long>("Id");
                            string name = reader.GetFieldValue<string>("Name");
                            DateTime timestamp = reader.GetFieldValue<DateTime>("CommitTs");
                            Console.WriteLine($"{id}: {name} - {timestamp:HH:mm:ss.ffffff}");
                        }
                    }
                }
                // End sample
            });
        }

        [Fact]
        public async Task ReadUpdateDeleteAsync()
        {
            string connectionString = _fixture.ConnectionString;

            await RetryHelpers.RetryOnceAsync(async () =>
            {
                // Sample: ReadUpdateDeleteAsync
                // Additional: CreateUpdateCommand
                // Additional: CreateDeleteCommand
                // Additional: CreateSelectCommand
                using (SpannerConnection connection = new SpannerConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Read the first two keys in the database.
                    List<string> keys = new List<string>();
                    SpannerCommand selectCmd = connection.CreateSelectCommand("SELECT * FROM TestTable");
                    using (SpannerDataReader reader = await selectCmd.ExecuteReaderAsync())
                    {
                        while (keys.Count < 3 && await reader.ReadAsync())
                        {
                            keys.Add(reader.GetFieldValue<string>("Key"));
                        }
                    }

                    // Update the Int64Value of keys[0]
                    // Include the primary key and update columns.
                    SpannerCommand updateCmd = connection.CreateUpdateCommand(
                        "TestTable", new SpannerParameterCollection
                        {
                        {"Key", SpannerDbType.String, keys[0]},
                        {"Int64Value", SpannerDbType.Int64, 0L}
                        });
                    await updateCmd.ExecuteNonQueryAsync();

                    // Delete row for keys[1]
                    SpannerCommand deleteCmd = connection.CreateDeleteCommand(
                        "TestTable", new SpannerParameterCollection
                        {
                        {"Key", SpannerDbType.String, keys[1]}
                        });
                    await deleteCmd.ExecuteNonQueryAsync();
                }
                // End sample
            });
        }

        // Sample: SpannerFaultDetectionStrategy
        private class SpannerFaultDetectionStrategy : ITransientErrorDetectionStrategy
        {
            /// <inheritdoc />
            public bool IsTransient(Exception ex) => ex.IsTransientSpannerFault();
        }
        // End sample

        [Fact]
        public async Task TransactionAsync()
        {
            string connectionString = _fixture.ConnectionString;

            // Sample: TransactionAsync
            // Additional: BeginTransactionAsync
            RetryPolicy<SpannerFaultDetectionStrategy> retryPolicy =
                new RetryPolicy<SpannerFaultDetectionStrategy>(RetryStrategy.DefaultExponential);

            await retryPolicy.ExecuteAsync(
                async () =>
                {
                    using (SpannerConnection connection = new SpannerConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        using (SpannerTransaction transaction = await connection.BeginTransactionAsync())
                        {
                            SpannerCommand cmd = connection.CreateInsertCommand(
                                "TestTable", new SpannerParameterCollection
                                {
                                    {"Key", SpannerDbType.String},
                                    {"StringValue", SpannerDbType.String},
                                    {"Int64Value", SpannerDbType.Int64}
                                });
                            cmd.Transaction = transaction;

                            // This executes a single transactions with alls row written at once during CommitAsync().
                            // If a transient fault occurs, this entire method is re-run.
                            for (int i = 0; i < 5; i++)
                            {
                                cmd.Parameters["Key"].Value = Guid.NewGuid().ToString("N");
                                cmd.Parameters["StringValue"].Value = $"StringValue{i}";
                                cmd.Parameters["Int64Value"].Value = i;
                                await cmd.ExecuteNonQueryAsync();
                            }

                            await transaction.CommitAsync();
                        }
                    }
                });
            // End sample
        }

#if !NETCOREAPP1_0
        [Fact]
        public async Task TransactionScopeAsync()
        {
            string connectionString = _fixture.ConnectionString;

            // Sample: TransactionScopeAsync
            // Additional: CreateInsertCommand
            RetryPolicy<SpannerFaultDetectionStrategy> retryPolicy =
                new RetryPolicy<SpannerFaultDetectionStrategy>(RetryStrategy.DefaultExponential);

            await retryPolicy.ExecuteAsync(
                async () =>
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        using (SpannerConnection connection = new SpannerConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            SpannerCommand cmd = connection.CreateInsertCommand(
                                "TestTable", new SpannerParameterCollection
                                {
                                    {"Key", SpannerDbType.String},
                                    {"StringValue", SpannerDbType.String},
                                    {"Int64Value", SpannerDbType.Int64}
                                });

                            // This executes a single transactions with alls row written at once during scope.Complete().
                            // If a transient fault occurs, this entire method is re-run.
                            for (int i = 0; i < 5; i++)
                            {
                                cmd.Parameters["Key"].Value = Guid.NewGuid().ToString("N");
                                cmd.Parameters["StringValue"].Value = $"StringValue{i}";
                                cmd.Parameters["Int64Value"].Value = i;
                                await cmd.ExecuteNonQueryAsync();
                            }

                            scope.Complete();
                        }
                    }
                });
            // End sample
        }

        [Fact]
        public void DataAdapter()
        {
            string connectionString = _fixture.ConnectionString;

            RetryHelpers.RetryOnce(() =>
            {
                // Sample: DataAdapter
                using (SpannerConnection connection = new SpannerConnection(connectionString))
                {
                    DataSet untypedDataSet = new DataSet();

                    // Provide the name of the Cloud Spanner table and primary key column names.
                    SpannerDataAdapter adapter = new SpannerDataAdapter(connection, "TestTable", "Key");
                    adapter.Fill(untypedDataSet);

                    // Insert a row
                    DataRow row = untypedDataSet.Tables[0].NewRow();
                    row["Key"] = Guid.NewGuid().ToString("N");
                    row["StringValue"] = "New String Value";
                    row["Int64Value"] = 0L;
                    untypedDataSet.Tables[0].Rows.Add(row);

                    adapter.Update(untypedDataSet.Tables[0]);
                }
                // End sample
            });
        }
#endif
    }
}

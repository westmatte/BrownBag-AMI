using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BBConsoleApp
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello Valhalla!");

                BuildConfig();

                var dbResource = Configuration["AMIResources:SQLServer"];
                var dbAccessToken = await GetAccessTokenAsync(dbResource);

                var employees = await GetEmployeeAsync(dbAccessToken);

                await WriteEmployeesToBlob(employees);

                foreach (var employee in employees)
                {
                    Console.WriteLine(employee.ToString());
                }

                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
            }
        }

        private static async Task WriteEmployeesToBlob(IEnumerable<Employee> employees)
        {
            var storageResource = Configuration["AMIResources:Storage"];
            var storageAccessToken = await GetAccessTokenAsync(storageResource);
            var tokenCredential = new TokenCredential(storageAccessToken);
            var credentials = new StorageCredentials(tokenCredential);

            var blobEndpoint = new Uri(Configuration["StorageAccount:BlobEndpoint"]);
            var client = new CloudBlobClient(blobEndpoint, credentials);
            var container = client.GetContainerReference(Configuration["StorageAccount:Container"]);
            var blob = container.GetBlockBlobReference("Employees.txt");

            using (var stream = await blob.OpenWriteAsync())
            using (var writer = new StreamWriter(stream))
            {
                foreach (var employee in employees)
                {
                    await writer.WriteLineAsync(employee.ToString());
                }
            }
        }

        public static async Task<string> GetAccessTokenAsync(string resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(resource);

            return accessToken;
        }

        public static async Task<SqlConnection> GetSqlConnectionAsync(string accessToken)
        {
            var connection = new SqlConnection
            {
                AccessToken = accessToken,
                ConnectionString = Configuration["SqlConnectionString"]
            };

            await connection.OpenAsync();

            return connection;
        }

        public static async Task<IEnumerable<Employee>> GetEmployeeAsync(string accessToken)
        {
            using (var connection = await GetSqlConnectionAsync(accessToken))
            {
                var sql = "SELECT * FROM dbo.EMPLOYEE";
                var employees = await connection.QueryAsync<Employee>(sql);

                return employees;
            }
        }

        private static void BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            builder.AddAzureKeyVault(Configuration["KeyVaultUrl"]);
            Configuration = builder.Build();
        }
    }
}
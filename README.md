# BrownBag-AMI
A repo with documents and example of how to implement a few applications to use Azure Manage Identity to communicate between resources and keep credentials out of code.

## ISSUES
There seems to be an issue where when running this console app on an azure VM the Microsoft.Azure.Services.AppAuthentication is unable to reach the Azure Instance Metadata Service endpoint, and thus unable to retrieve an access token. I'm not sure if this issue is because of the application running .NET Core 2.2 preview 3 and all NuGet packages using the latest previews, or if there is an issue with the way I set up the VM. In other words, just use this application as a sample and a little bit of inspiration of how to get started using Azure Managed Identity in your .NET applications to communicate with other Azure Resources. 

## Introduction
This is a demo application with some concrete examples how to get an Access Token using Azure Managed Identity to communicate with an Azure KeyVault, Azure SQL DB and Azure Storage Account (in this case a blob container).

The application first loads all the secrets from a Key Vault (the Key Vault which url is specified in the appsettings.json) into the appsettings.json config file for easy access. Then retrieve another token to access the Azure SQL DB and retrieve a list of all Employees in the dbo.Employee table.

Next, retrieve another token to access the specified Storage Account and Container, and then write the info retrieved from the DB to a blob in the storage account. All this without any credentials specified in the code. Everything in this repo is using preview features and are subject to change. 

This solution uses the AppAuthentication NuGet to retrieve access tokens, like this:
```
public static async Task<string> GetAccessTokenAsync(string resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(resource);

            return accessToken;
        }
```
A link to available resource endpoints are specified in the References / Further reading section at the end of the README. 

## Prerequisites
.NET Core 2.2 preview 3\
An Azure Subscription

## Get Started
To use this application.

### Create an Azure Key Vault in your Azure subscription.
Save the baseUrl of the KeyVault and add it to the appsettings.json "KeyVaultUrl".

Go to Access Policies and add the user / application to get access to the secrets in the keyvault.

### Create an Azure SQL Database
Save the connection string to the database to the created KeyVault and name the secret to SqlConnectionString.\
Format the connection string like this:
#### Data Source=<sqlserverurl>;Initial Catalog=<databasename>;Connect Timeout=30

Enable Active Directory on the Azure SQL Server

#### Create a table you wish to read from and add an identity (user/application) from the Azure Active Directory
  ```
  CREATE TABLE dbo.Employee 
  (
    EmployeeId int not null Primary Key IDENTITY(1, 1),
    EmployeeName nvarchar(255) not null,
    Address nvarchar(255) null
  )
  GO

  insert into dbo.Employee values ('Kalle Anka', 'Ankeborg')

  --Note that you need to be logged in as an Active Directory Admin to add a user from an external provider
  CREATE USER [] FROM EXTERNAL PROVIDER
  GO

  ALTER ROLE db_datareader ADD MEMBER []
  GO
  ```

### Create an Azure Storage Account and a blob container
Save the blob endpoint to the appsettings.json or the azure key vault 
#### (if saved in keyvault, set the secretname to StorageAccount--BlobEndpoint)
Save the container name to the appsettings.json

Navigate to the container or the storage account and give the user/application the STORAGE BLOB DATA CONTRIBUTOR (PREVIEW) role to be able to write to the blob.

### To run the application in development
Run the application on your local machine with the correct account in Visual Studio, or by logging in using the Azure CLI. This account needs to have access to all of the above resources as specified in the example above.

### To run the application in production (note, read the issues in the beginning of the README)
Enable the Managed Identity of the VM, and give the identity access to the key vault secret, add the VM identity to the Azure SQL DB and the storage account role, as specified in the example above.

To publish the project to run on an Windows / Ubuntu machine, use the cmd below respectively:
dotnet publish -c Release -r win10-x64
dotnet publish -c Release -r ubuntu.16.04-x64

## NuGet Dependencies
Dapper - v1.50.5
Microsoft.Azure.KeyVault.Core - v3.0.0
Microsoft.Azure.Services.AppAuthentication - v1.0.1
Microsoft.Extensions.Configuration - v2.1.1
Microsoft.Extensions.Configuration.AzureKeyVault - v2.1.1
Microsoft.Extensions.Configuration.FileExtensions - v2.1.1
Microsoft.Extensions.Configuration.Json - v2.1.1
Microsoft.NETCore.App - v2.2.0-preview3-27014-02
System.Data.SqlClient - v4.6.0-preview3-27014-02
WindowsAzure.Storage - v9.3.2


## References / Further reading
To dive deeper into this topic, here are some more resources that I recommend reading.

Introduction to Azure Managed Identity - https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview
Get Access Tokens - https://docs.microsoft.com/sv-se/azure/active-directory/managed-identities-azure-resources/how-to-use-vm-token
AppAuthentication NuGet - https://docs.microsoft.com/sv-se/azure/key-vault/service-to-service-authentication
Available Services and Endpoints - https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-msi
Azure Instance Metadata Service endpoint - https://docs.microsoft.com/sv-se/azure/virtual-machines/windows/instance-metadata-service 

Microsoft Code Examples:
AppService Managed Identity - https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet
Linux VM Managed Identity - https://github.com/Azure-Samples/linuxvm-msi-keyvault-arm-dotnet


using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace VizarLibrary.DataAccess;

public static partial class Secrets
{
    public static string DatabaseName => "Vizar";

    public static string AzureConnectionString = GetSecret(nameof(AzureConnectionString));
    public static string LocalConnectionString => "Data Source=AADILAPIKIIT;Initial Catalog=Vizar;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

    public static string AzureBlobStorageAccountName => "vizar";
    public static string AzureBlobStorageConnectionString = GetSecret(nameof(AzureBlobStorageConnectionString));
    public static string AzureBlobStorageAccountKey = GetSecret(nameof(AzureBlobStorageAccountKey));

    public static string SyncfusionLicense = GetSecret(nameof(SyncfusionLicense));

    public static string Email => "softaadi@gmail.com";
    public static string EmailPassword = GetSecret(nameof(EmailPassword));

    public static string ToEmail = GetSecret(nameof(ToEmail));
    public static string ToName => "Vizar";

    public static string OnlineFullLogoPath => "https://raw.githubusercontent.com/aadipoddar/Vizar/refs/heads/main/Vizar/Vizar.Web/wwwroot/images/logo_full.png";
    public static string AadiSoftWebsite => "https://aadisoft.vercel.app";
    public static string AppWebsite => "https://vizar.azurewebsites.net";

    private static string GetSecret(string key) =>
        new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .Build()
            .GetSection(key).Value;
}
csharp
public partial class IrAttachment
{
    private static readonly Regex CloudStorageAzureUrlPattern = new Regex(@"https://(?<accountName>[\w]+).blob.core.windows.net/(?<containerName>[\w]+)/(?<blobName>[^?]+)");
    private static Dictionary<string, (Dictionary<string, object> Config, object UserDelegationKey)> CloudStorageAzureUserDelegationKeys = new Dictionary<string, (Dictionary<string, object>, object)>();

    public Dictionary<string, string> GetCloudStorageAzureInfo()
    {
        var match = CloudStorageAzureUrlPattern.Match(this.Url);
        if (!match.Success)
        {
            throw new ValidationException($"{this.Url} is not a valid Azure Blob Storage URL.");
        }
        return new Dictionary<string, string>
        {
            ["AccountName"] = match.Groups["accountName"].Value,
            ["ContainerName"] = match.Groups["containerName"].Value,
            ["BlobName"] = Uri.UnescapeDataString(match.Groups["blobName"].Value)
        };
    }

    public object GetCloudStorageAzureUserDelegationKey()
    {
        var dbName = Env.Registry.DbName;
        var (cachedConfig, cachedUserDelegationKey) = CloudStorageAzureUserDelegationKeys.TryGetValue(dbName, out var value) ? value : (null, null);
        
        var dbConfig = Env.Get<ResConfigSettings>().GetCloudStorageConfiguration();
        dbConfig.Remove("ContainerName");
        var icp = Env.Get<IrConfigParameter>().Sudo();
        dbConfig["Sequence"] = int.Parse(icp.GetParam("cloud_storage_azure_user_delegation_key_sequence", "0"));

        if (dbConfig.Equals(cachedConfig))
        {
            if (cachedUserDelegationKey is Exception exception)
            {
                throw exception;
            }
            if (cachedUserDelegationKey != null)
            {
                var expiry = DateTime.ParseExact(((dynamic)cachedUserDelegationKey).SignedExpiry, "yyyy-MM-ddTHH:mm:ssZ", null, System.Globalization.DateTimeStyles.AssumeUniversal);
                if (expiry > DateTime.UtcNow.AddDays(1))
                {
                    return cachedUserDelegationKey;
                }
            }
        }

        var keyStartTime = DateTime.UtcNow;
        var keyExpiryTime = keyStartTime.AddDays(7);

        try
        {
            var userDelegationKey = GetUserDelegationKey(
                tenantId: (string)dbConfig["TenantId"],
                clientId: (string)dbConfig["ClientId"],
                clientSecret: (string)dbConfig["ClientSecret"],
                accountName: (string)dbConfig["AccountName"],
                keyStartTime: keyStartTime,
                keyExpiryTime: keyExpiryTime
            );
            CloudStorageAzureUserDelegationKeys[dbName] = (dbConfig, userDelegationKey);
            return userDelegationKey;
        }
        catch (ClientAuthenticationException e)
        {
            var validationException = new ValidationException(e.Message);
            CloudStorageAzureUserDelegationKeys[dbName] = (dbConfig, validationException);
            throw validationException;
        }
    }

    public string GenerateCloudStorageAzureUrl(string blobName)
    {
        var icp = Env.Get<IrConfigParameter>().Sudo();
        var accountName = icp.GetParam("cloud_storage_azure_account_name");
        var containerName = icp.GetParam("cloud_storage_azure_container_name");
        return $"https://{accountName}.blob.core.windows.net/{containerName}/{Uri.EscapeDataString(blobName)}";
    }

    public string GenerateCloudStorageAzureSasUrl(Dictionary<string, object> kwargs)
    {
        var token = GenerateBlobSas(userDelegationKey: GetCloudStorageAzureUserDelegationKey(), kwargs);
        return $"{GenerateCloudStorageAzureUrl((string)kwargs["BlobName"])}?{token}";
    }

    public string GenerateCloudStorageUrl()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "azure")
        {
            return base.GenerateCloudStorageUrl();
        }
        var blobName = GenerateCloudStorageBlobName();
        return GenerateCloudStorageAzureUrl(blobName);
    }

    public Dictionary<string, object> GenerateCloudStorageDownloadInfo()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "azure")
        {
            return base.GenerateCloudStorageDownloadInfo();
        }
        var info = GetCloudStorageAzureInfo();
        var expiry = DateTime.UtcNow.AddSeconds(CloudStorageDownloadUrlTimeToExpiry);
        return new Dictionary<string, object>
        {
            ["Url"] = GenerateCloudStorageAzureSasUrl(new Dictionary<string, object>(info)
            {
                ["Permission"] = "r",
                ["Expiry"] = expiry,
                ["CacheControl"] = $"private, max-age={CloudStorageDownloadUrlTimeToExpiry}"
            }),
            ["TimeToExpiry"] = CloudStorageDownloadUrlTimeToExpiry
        };
    }

    public Dictionary<string, object> GenerateCloudStorageUploadInfo()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "azure")
        {
            return base.GenerateCloudStorageUploadInfo();
        }
        var info = GetCloudStorageAzureInfo();
        var expiry = DateTime.UtcNow.AddSeconds(CloudStorageUploadUrlTimeToExpiry);
        var url = GenerateCloudStorageAzureSasUrl(new Dictionary<string, object>(info)
        {
            ["Permission"] = "c",
            ["Expiry"] = expiry
        });
        return new Dictionary<string, object>
        {
            ["Url"] = url,
            ["Method"] = "PUT",
            ["Headers"] = new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob"
            },
            ["ResponseStatus"] = 201
        };
    }
}

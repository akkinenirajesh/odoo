csharp
public partial class IrAttachment
{
    private static readonly Regex CloudStorageGoogleUrlPattern = new Regex(@"https://storage\.googleapis\.com/(?<bucket_name>[\w\-.]+)/(?<blob_name>[^?]+)");
    private static Dictionary<string, (string AccountInfo, object Credential)> CloudStorageCredentials = new Dictionary<string, (string, object)>();

    public CloudStorageGoogleInfo GetCloudStorageGoogleInfo()
    {
        var match = CloudStorageGoogleUrlPattern.Match(this.Url);
        if (!match.Success)
        {
            throw new ValidationException($"{this.Url} is not a valid Google Cloud Storage URL.");
        }
        return new CloudStorageGoogleInfo
        {
            BucketName = match.Groups["bucket_name"].Value,
            BlobName = Uri.UnescapeDataString(match.Groups["blob_name"].Value)
        };
    }

    public object GetCloudStorageGoogleCredential()
    {
        var dbName = Env.Registry.DbName;
        var accountInfo = Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_google_account_info");

        if (CloudStorageCredentials.TryGetValue(dbName, out var cachedInfo) && cachedInfo.AccountInfo == accountInfo)
        {
            return cachedInfo.Credential;
        }

        // Note: You'll need to implement the actual credential creation logic here
        var credential = CreateGoogleCredential(accountInfo);
        CloudStorageCredentials[dbName] = (accountInfo, credential);
        return credential;
    }

    public string GenerateCloudStorageGoogleUrl(string blobName)
    {
        var bucketName = Env.Get<IrConfigParameter>().GetParam("cloud_storage_google_bucket_name");
        return $"https://storage.googleapis.com/{bucketName}/{Uri.EscapeDataString(blobName)}";
    }

    public string GenerateCloudStorageGoogleSignedUrl(string bucketName, string blobName, Dictionary<string, object> options)
    {
        var quoteBlobName = Uri.EscapeDataString(blobName);
        return GenerateSignedUrlV4(
            GetCloudStorageGoogleCredential(),
            $"/{bucketName}/{quoteBlobName}",
            options
        );
    }

    public override string GenerateCloudStorageUrl()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "google")
        {
            return base.GenerateCloudStorageUrl();
        }
        var blobName = GenerateCloudStorageBlobName();
        return GenerateCloudStorageGoogleUrl(blobName);
    }

    public override CloudStorageDownloadInfo GenerateCloudStorageDownloadInfo()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "google")
        {
            return base.GenerateCloudStorageDownloadInfo();
        }
        var info = GetCloudStorageGoogleInfo();
        return new CloudStorageDownloadInfo
        {
            Url = GenerateCloudStorageGoogleSignedUrl(info.BucketName, info.BlobName, new Dictionary<string, object>
            {
                { "method", "GET" },
                { "expiration", CloudStorageDownloadUrlTimeToExpiry }
            }),
            TimeToExpiry = CloudStorageDownloadUrlTimeToExpiry
        };
    }

    public override CloudStorageUploadInfo GenerateCloudStorageUploadInfo()
    {
        if (Env.Get<IrConfigParameter>().Sudo().GetParam("cloud_storage_provider") != "google")
        {
            return base.GenerateCloudStorageUploadInfo();
        }
        var info = GetCloudStorageGoogleInfo();
        return new CloudStorageUploadInfo
        {
            Url = GenerateCloudStorageGoogleSignedUrl(info.BucketName, info.BlobName, new Dictionary<string, object>
            {
                { "method", "PUT" },
                { "expiration", CloudStorageUploadUrlTimeToExpiry }
            }),
            Method = "PUT",
            ResponseStatus = 200
        };
    }

    // Helper methods and classes
    private object CreateGoogleCredential(string accountInfo)
    {
        // Implement Google credential creation logic
        throw new NotImplementedException();
    }

    private string GenerateSignedUrlV4(object credentials, string resource, Dictionary<string, object> options)
    {
        // Implement signed URL generation logic
        throw new NotImplementedException();
    }
}

public class CloudStorageGoogleInfo
{
    public string BucketName { get; set; }
    public string BlobName { get; set; }
}

public class CloudStorageDownloadInfo
{
    public string Url { get; set; }
    public int TimeToExpiry { get; set; }
}

public class CloudStorageUploadInfo
{
    public string Url { get; set; }
    public string Method { get; set; }
    public int ResponseStatus { get; set; }
}

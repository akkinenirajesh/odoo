csharp
public partial class CloudStorageAttachment
{
    private const int CloudStorageUploadUrlTimeToExpiry = 300; // 300 seconds
    private const int CloudStorageDownloadUrlTimeToExpiry = 300; // 300 seconds

    public Stream ToHttpStream()
    {
        if (Type == AttachmentType.CloudStorage && Env.ResConfigSettings.GetCloudStorageConfiguration())
        {
            var info = GenerateCloudStorageDownloadInfo();
            var stream = new Stream(type: "url", url: info["url"]);
            if (info.ContainsKey("time_to_expiry"))
            {
                // cache the redirection until 10 seconds before the expiry
                stream.MaxAge = Math.Max(info["time_to_expiry"] - 10, 0);
            }
            return stream;
        }
        return base.ToHttpStream();
    }

    public void PostAddCreate(bool cloudStorage = false)
    {
        base.PostAddCreate();
        if (cloudStorage)
        {
            if (!Env.IrConfigParameter.GetParam("cloud_storage_provider"))
            {
                throw new UserError("Cloud Storage is not enabled");
            }
            Raw = false;
            Type = AttachmentType.CloudStorage;
            Url = GenerateCloudStorageUrl();
        }
    }

    public string GenerateCloudStorageBlobName()
    {
        return $"{Id}/{Guid.NewGuid()}/{Name}";
    }

    public string GenerateCloudStorageUrl()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GenerateCloudStorageDownloadInfo()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GenerateCloudStorageUploadInfo()
    {
        throw new NotImplementedException();
    }
}

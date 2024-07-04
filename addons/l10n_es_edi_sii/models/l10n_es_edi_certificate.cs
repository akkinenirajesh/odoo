csharp
using System;
using System.Linq;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;

public partial class Certificate
{
    public override string ToString()
    {
        return DateStart.ToString();
    }

    public (byte[], byte[], X509Certificate) DecodeCertificate()
    {
        if (string.IsNullOrEmpty(Password))
        {
            return (null, null, null);
        }

        try
        {
            var store = new Pkcs12Store(new System.IO.MemoryStream(Convert.FromBase64String(Content)), Password.ToCharArray());
            var alias = store.Aliases.Cast<string>().FirstOrDefault(a => store.IsKeyEntry(a));

            if (alias == null)
            {
                throw new Exception("No private key found in the certificate.");
            }

            var certificate = store.GetCertificate(alias).Certificate;
            var privateKey = store.GetKey(alias).Key;

            var pemCertificate = certificate.GetEncoded();
            var pemPrivateKey = privateKey.GetEncoded();

            return (pemCertificate, pemPrivateKey, certificate);
        }
        catch (Exception)
        {
            throw new ValidationException("There has been a problem with the certificate, some usual problems can be:\n" +
                "- The password given or the certificate are not valid.\n" +
                "- The certificate content is invalid.");
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();

        var spainTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var spainDt = TimeZoneInfo.ConvertTime(DateTime.Now, spainTz);

        var (_, _, certif) = DecodeCertificate();

        DateStart = TimeZoneInfo.ConvertTime(certif.NotBefore, spainTz);
        DateEnd = TimeZoneInfo.ConvertTime(certif.NotAfter, spainTz);

        if (spainDt > DateEnd)
        {
            throw new ValidationException($"The certificate is expired since {DateEnd}");
        }
    }

    private DateTime GetEsCurrentDatetime()
    {
        var spainTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        return TimeZoneInfo.ConvertTime(DateTime.Now, spainTz);
    }
}

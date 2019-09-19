namespace IAI {

    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    class X509CertificateHelper {

        public const string OPC_TWIN_DNS_NAME = "opctwin.services.net";

        public static X509Certificate2 BuildSelfSignedServerCertificate(
            string certificateName,
            string dnsName,
            string password
        ) {
            var sanBuilder = new SubjectAlternativeNameBuilder();

            //sanBuilder.AddIpAddress(IPAddress.Loopback);
            //sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            
            //sanBuilder.AddDnsName("localhost");
            //sanBuilder.AddDnsName(Environment.MachineName);
            sanBuilder.AddDnsName(dnsName);

            var distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using (var rsa = RSA.Create(2048)) {
                var request = new CertificateRequest(
                    distinguishedName,
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature,
                        false
                    )
                );

                var serverAuthenticationOid = new Oid("1.3.6.1.5.5.7.3.1");
                var clientAuthenticationOid = new Oid("1.3.6.1.5.5.7.3.2");

                request.CertificateExtensions.Add(
                   new X509EnhancedKeyUsageExtension(
                       new OidCollection {
                           serverAuthenticationOid,
                           clientAuthenticationOid
                       },
                       false
                   )
               );

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(
                    new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                    new DateTimeOffset(DateTime.UtcNow.AddYears(1))
                );

                certificate.FriendlyName = certificateName;

                var x509Certificate2 = new X509Certificate2(
                    certificate.Export(X509ContentType.Pfx, password),
                    password,
                    X509KeyStorageFlags.MachineKeySet
                );

                return x509Certificate2;
            }
        }

        public static void AddCertificateToUserPersonalStore(X509Certificate2 cert) {
            using(var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser)) {
                certStore.Open(OpenFlags.ReadWrite);
                certStore.Add(cert);
            }
        }
    }
}

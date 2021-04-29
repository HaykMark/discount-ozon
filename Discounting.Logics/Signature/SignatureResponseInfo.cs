using System;

namespace Discounting.Logics
{
    public class SignatureResponseInfo
    {
        public string Serial { get; set; }
        public string Thumbprint { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTill { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public SignatureResponseCertInfo Subject { get; set; }
        public SignatureResponseCertInfo Issuer { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string OGRN { get; set; }
        public string SNILS { get; set; }
    }

    public class SignatureResponseCertInfo
    {
        
        public string UnstructuredName { get; set; }
        public string EmailAddress { get; set; }
        public string INN { get; set; }
        public string SNILS { get; set; }
        public string OGRN { get; set; }
        public string Title { get; set; }
        public string Street { get; set; }
        public string O { get; set; }
        public string L { get; set; }
        public string ST { get; set; }
        public string C { get; set; }
        public string CN { get; set; }
        public string GN { get; set; }
        public string SN { get; set; }
    }
}
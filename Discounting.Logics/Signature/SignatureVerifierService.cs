using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.JsonConverter;
using Discounting.Entities;
using Discounting.Extensions;

namespace Discounting.Logics
{
    public class SignatureVerifierService
    {
        private readonly HttpClient httpClient;

        public SignatureVerifierService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<SignatureVerificationResponse> GetVerificationCenterResponseAsync(string signatureBase64,
            string originalDocumentBase64)
        {
            var content = new Dictionary<string, string>
            {
                {"signature", signatureBase64},
                {"detached", "true"},
                {"origin", originalDocumentBase64},
                {"nochain", "false"},
                {"norev", "true"}
            };

            try
            {
                var encodedItems =
                    content.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
                var encodedContent = new StringContent(string.Join("&", encodedItems), null,
                    "application/x-www-form-urlencoded");

                var response = await httpClient.PostAsync("verifysignature", encodedContent);
                var result =
                    await response.GetContentAsync<SignatureVerificationResponse>(JsonSerializationSettingsProvider
                        .GetSerializeSettings());
                response.EnsureSuccessStatusCode();
                return result;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TimeoutException)
                {
                    ex = ex.InnerException;
                }
                else if (ex is TaskCanceledException canceledException)
                {
                    if (canceledException.CancellationToken.IsCancellationRequested == false)
                    {
                        ex = new TimeoutException("Timeout occurred");
                    }
                }

                throw new HttpException(504,
                    new UnknownErrorDetails(
                        $"Exception at calling verifysignature :{ex.Message} + InnerException {ex.InnerException}"));
            }
        }

        public SignatureInfo GetSignatureInfo(SignatureVerificationResponse verificationResponse)
        {
            if (verificationResponse.Data == null)
                return new SignatureInfo();
            return new SignatureInfo
            {
                Company = verificationResponse.Data.Company,
                Email = verificationResponse.Data.Email,
                Name = verificationResponse.Data.Name,
                Serial = verificationResponse.Data.Serial,
                Thumbprint = verificationResponse.Data.Thumbprint,
                ValidFrom = verificationResponse.Data.ValidFrom,
                ValidTill = verificationResponse.Data.ValidTill,
                INN = verificationResponse.Data.INN,
                OGRN = verificationResponse.Data.OGRN,
                SNILS = verificationResponse.Data.SNILS
            };
        }

        public string GetInnFromVerificationResponse(SignatureResponseInfo responseInfo)
        {
            if (!string.IsNullOrEmpty(responseInfo.INN))
            {
                return responseInfo.INN.Trim();
            }

            if (responseInfo.Subject != null && !string.IsNullOrEmpty(responseInfo.Subject.INN))
            {
                return responseInfo.Subject.INN.Trim();
            }

            return null;
        }
    }
    

    public class SignatureVerificationResponse
    {
        public bool Success { get; set; }

        public string Result { get; set; }
        public SignatureResponseInfo Data { get; set; }
    }
}
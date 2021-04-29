using System;
using System.IO;
using Discounting.Common.Options;
using Discounting.Entities;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Entities.Templates;
using Discounting.Extensions;
using Microsoft.Extensions.Options;

namespace Discounting.Logics
{
    public interface IUploadPathProviderService
    {
        string GetUploadPath(Guid id, string contentType, UploadProvider type);
        string GetTemplatePath(Guid id, string contentType, TemplateType type);
        string GetRegistryTemplateDestinationPath(string fileName);
        string GetCompanyRegulationDestinationPath(Guid id, string contentType, CompanyRegulationType type);
        string GetUserProfileRegulationDestinationPath(Guid id);
        string GetSignaturePath(string fileName, SignatureType type);
    }

    public class UploadPathProviderService : IUploadPathProviderService
    {
        private readonly UploadOptions uploadOptions;

        public UploadPathProviderService(IOptions<UploadOptions> uploadOptions)
        {
            this.uploadOptions = uploadOptions.Value;
        }

        public string GetUploadPath(Guid id, string contentType, UploadProvider type)
        {
            var ext = MimeTypeMap.GetExtension(contentType);
            var destinationFolder = type switch
            {
                UploadProvider.Supply => uploadOptions.SupplyPath,
                UploadProvider.Registry => uploadOptions.RegistryPath,
                UploadProvider.Regulation => uploadOptions.RegulationPath,
                UploadProvider.UnformalizedDocument => uploadOptions.UnformalizedDocumentPath,
                UploadProvider.UserRegulation => uploadOptions.UserRegulationPath,
                _ => null
            };
            return GetPath(destinationFolder, id.ToString(), ext);
        }

        public string GetTemplatePath(Guid id, string contentType, TemplateType type)
        {
            var ext = MimeTypeMap.GetExtension(contentType);
            var destinationFolder = type switch
            {
                TemplateType.Registry => uploadOptions.RegistryTemplatePath,
                TemplateType.Verification => uploadOptions.VerificationTemplatePath,
                TemplateType.Discount => uploadOptions.DiscountTemplatePath,
                TemplateType.ProfileRegulationSellerBuyer => uploadOptions.ProfileRegulationTemplatePath,
                TemplateType.ProfileRegulationPrivateCompany => uploadOptions.ProfileRegulationTemplatePath,
                TemplateType.ProfileRegulationBank => uploadOptions.ProfileRegulationTemplatePath,
                TemplateType.ProfileRegulationUser => uploadOptions.ProfileRegulationTemplatePath,
                _ => null
            };
            return GetPath(destinationFolder, id.ToString(), ext);
        }

        public string GetRegistryTemplateDestinationPath(string fileName)
        {
            return Path.Combine(uploadOptions.Path, uploadOptions.RegistryPath, fileName);
        }

        public string GetCompanyRegulationDestinationPath(Guid id, string contentType, CompanyRegulationType type)
        {
            var ext = MimeTypeMap.GetExtension(contentType);
            var destinationFolder = Path.Combine(uploadOptions.RegulationPath, type.ToString());
            return GetPath(destinationFolder, id.ToString(), ext);
        }

        public string GetUserProfileRegulationDestinationPath(Guid id)
        {
            const string ext = ".xlsx";
            var destinationFolder =
                Path.Combine(uploadOptions.UserRegulationPath, UserRegulationType.Profile.ToString());
            return GetPath(destinationFolder, id.ToString(), ext);
        }

        public string GetSignaturePath(string fileName, SignatureType type)
        {
            const string ext = ".sgn";
            return GetPath(Path.Combine(uploadOptions.SignaturePath, type.ToString()), fileName, ext);
        }

        private string GetPath(string destinationFolder, string fileName, string ext)
        {
            if (!string.IsNullOrEmpty(destinationFolder) &&
                !Directory.Exists(Path.Combine(uploadOptions.Path, destinationFolder)))
            {
                Directory.CreateDirectory(Path.Combine(uploadOptions.Path, destinationFolder));
            }

            return string.IsNullOrEmpty(destinationFolder)
                ? Path.Combine(uploadOptions.Path, fileName + ext)
                : Path.Combine(uploadOptions.Path, destinationFolder, fileName + ext);
        }
    }
}
﻿ namespace Discounting.Common.Options
{
    /// <summary>
    /// Upload options model class for image and file upload.
    /// </summary>
    public class UploadOptions
    {
        public string Path { get; set; }
        public string RegistryTemplatePath { get; set; }
        public string DiscountTemplatePath { get; set; }
        public string VerificationTemplatePath { get; set; }
        public string ProfileRegulationTemplatePath { get; set; }
        public string RegistryPath { get; set; }
        public string SignaturePath { get; set; }
        public string RegulationPath { get; set; }
        public string UnformalizedDocumentPath { get; set; }
        public string UserRegulationPath { get; set; }
        public string SupplyPath { get; set; }
    }
}
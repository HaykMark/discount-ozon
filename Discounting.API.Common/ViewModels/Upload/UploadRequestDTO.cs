using System;
using Discounting.API.Common.CustomAttributes;
using Discounting.Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace Discounting.API.Common.ViewModels
{
    public class UploadRequestDTO
    {
        [CustomRequired]
        public Guid ProviderId { get; set; }
        [CustomRequired]
        public UploadProvider Provider { get; set; }
    }
}
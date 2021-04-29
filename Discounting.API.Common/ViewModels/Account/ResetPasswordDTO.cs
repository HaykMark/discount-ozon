using System;
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
	public class ResetPasswordDTO
	{
		[CustomRequired]
		public Guid UserId { get; set; }
		
		[CustomRequired]
		[CustomStringLength(100, MinimumLength = 6)]
		[CustomDataType(DataType.Password)]
		public string NewPassword { get; set; }

		[CustomRequired]
		public string ConfirmationCode { get; set; }
	}
}
using System.ComponentModel.DataAnnotations;
using Discounting.API.Common.CustomAttributes;

namespace Discounting.API.Common.ViewModels.Account
{
	public class ChangePasswordDTO
	{
		[CustomRequired]
		[CustomDataType(DataType.Password)]
		public string OldPassword { get; set; }

		[CustomRequired]
		[CustomStringLength(100, MinimumLength = 4)]
		[CustomDataType(DataType.Password)]
		public string NewPassword { get; set; }

		[CustomRequired]
		[CustomDataType(DataType.Password)]
		[Compare(nameof(NewPassword))]
		public string ConfirmPassword { get; set; }
	}
}
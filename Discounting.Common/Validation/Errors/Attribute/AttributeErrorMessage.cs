using Discounting.Common.Types;

namespace Discounting.Common.Validation.Errors.Attribute
 {
     public class AttributeErrorMessage
     {
         public AttributeType AttributeType { get; set; }
         public object Args { get; set; }
         public string Message { get; set; }
     }
 }
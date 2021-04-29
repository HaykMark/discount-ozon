using System;
using System.Collections.Generic;
using System.Text;

namespace Discounting.Common.Validation
{
    public interface IValidator<T>
    {
        bool IsValid(T entity);
    }
}

using System;
using Discounting.Common.Types;
using Discounting.Common.Validation;
using Discounting.Data.Context;

namespace Discounting.Logics.Validators
{
    public interface IReferenceValidator
    {
        ValidationError ValidateReference<T, TEntity>(
            Func<T, Guid> selector,
            T next,
            T prev,
            string fieldName,
            string message, 
            string key
        ) where TEntity : class;

        ValidationError ValidateReference<T, TEntity>(
            Func<T, Guid> selector,
            T next,
            T prev,
            string fieldName
        ) where TEntity : class;
    }

    public class ReferenceValidator : IReferenceValidator
    {
        private readonly IUnitOfWork unitOfWork;

        public ReferenceValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public ValidationError ValidateReference<T, TEntity>(
            Func<T, Guid> selector,
            T next,
            T prev,
            string fieldName
        ) where TEntity : class
        {
            return BaseValidateReference<T,TEntity>(
                selector, next, prev, fieldName);
        }

        public ValidationError ValidateReference<T, TEntity>(
            Func<T, Guid> selector,
            T next,
            T prev,
            string fieldName,
            string message,
            string key
        ) where TEntity : class
        {
            return BaseValidateReference<T,TEntity>(
                selector, next, prev, fieldName, message,key);
        }

        private ValidationError BaseValidateReference<T, TEntity>(
            Func<T, Guid> selector,
            T next,
            T prev,
            string fieldName,
            string message = null,
            string key = null
        ) where TEntity : class
        {
            var nextEntityId = selector(next);

            var entity = unitOfWork.Set<TEntity>().Find(nextEntityId);

            if (entity == null)
            {
                return new NotFoundValidationError(fieldName);
            }

            var softEntry = entity as IActivatable;
            if (prev != null && softEntry != null)
            {
                var prevEntityId = selector(prev);
                if (prevEntityId != nextEntityId && !softEntry.IsActive)
                {
                    if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(key))
                    {
                        return new ReferenceNotAllowedError(fieldName);
                    }
                    return new ReferenceNotAllowedError(fieldName, message, key);
                }
            }
            else if (softEntry != null && !softEntry.IsActive)
            {
                if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(key))
                {
                    return new ReferenceNotAllowedError(fieldName);
                }
                return new ReferenceNotAllowedError(fieldName, message, key);
            }

            return null;
        }
    }
}
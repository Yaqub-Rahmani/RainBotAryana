using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.CrossCuttingConcerns
{
    public static class ValidationTool
    {
        public static void Validate(IValidator validator, object entity)
        {
            var result = validator.Validate(new ValidationContext<object>(entity));
            if (!result.IsValid) throw new FluentValidation.ValidationException(result.Errors);
        }
    }
}

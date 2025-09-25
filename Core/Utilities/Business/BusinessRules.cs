using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Business
{
    using Core.Utilities.Results;

    public static class BusinessRules
    {
        // Kurallar dizisinde ilk hata döner; hepsi başarılıysa null döner
        public static IResult Run(params IResult[] rules)
        {
            foreach (var rule in rules)
                if (rule is { Success: false })
                    return rule;
            return null;
        }
    }
}

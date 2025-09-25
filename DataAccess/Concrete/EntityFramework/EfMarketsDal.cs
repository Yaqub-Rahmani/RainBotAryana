using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfMarketsDal : EfEntityRepositoryBase<Market, RainBotContext>, IMarketsDal
    {
        public EfMarketsDal(RainBotContext context) : base(context) { }
    }
}

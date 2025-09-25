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
    //public class EfCategoryDal : EfEntityRepositoryBase<Category, RainBotContext>, ICategoryDal
    //{
    //    public EfCategoryDal(RainBotContext context) : base(context)
    //    {
    //    }

    //}

    public class EfCategoryDal : EfEntityRepositoryBase<Category, RainBotContext>, ICategoryDal
    {
        public EfCategoryDal(RainBotContext context) : base(context) { }

        public List<Category> GetAll()
        {
            throw new NotImplementedException();
        }
    }

}

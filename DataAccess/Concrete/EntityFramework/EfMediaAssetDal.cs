using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfMediaAssetDal : EfEntityRepositoryBase<MediaAsset, RainBotContext>, IMediaAssetDal
    {
        public EfMediaAssetDal(RainBotContext ctx) : base(ctx) { }

        public IQueryable<MediaAsset> Query() => _context.Set<MediaAsset>().AsQueryable();
        public IQueryable<MediaAsset> QueryNoTracking() => _context.Set<MediaAsset>().AsNoTracking();
    }
}

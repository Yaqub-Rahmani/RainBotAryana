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
    public class EfUserDal : EfEntityRepositoryBase<User, RainBotContext>, IUserDal
    {
        public EfUserDal(RainBotContext ctx) : base(ctx) { }

        public List<OperationClaim> GetClaims(User user)
        {
            var query =
                from oc in _context.OperationClaims
                join uoc in _context.UserOperationClaims on oc.Id equals uoc.OperationClaimId
                where uoc.UserId == user.Id
                select new OperationClaim { Id = oc.Id, Name = oc.Name };

            return query.ToList();
        }

        public IQueryable<User> QueryNoTracking() => _context.Set<User>().AsNoTracking();
    }
}

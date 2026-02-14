using Autofac.Core;
using Business.Abstract;
using Core.Aspects.Autofac.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserDal _userDal;
        public UserManager(IUserDal userDal) { _userDal = userDal; }
        public void Add(User user) => _userDal.Add(user);
        public User GetByMail(string email) => _userDal.Get(u => u.Email == email);
        public List<OperationClaim> GetClaims(User user) => _userDal.GetClaims(user);


        [CacheAspect(10)]
        public IDataResult<User> GetById(int id)
        {
            var e = _userDal.QueryNoTracking()
                        .FirstOrDefault(x => x.Id == id);
            return e == null ? new ErrorDataResult<User>(null, "یافت نشد.")
                             : new SuccessDataResult<User>(e);
        }

    }
}

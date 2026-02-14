using Core.Utilities.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IUserService
    {
        User GetByMail(string email);
        void Add(User user);
        List<OperationClaim> GetClaims(User user);
        IDataResult<User> GetById(int id);

    }
}

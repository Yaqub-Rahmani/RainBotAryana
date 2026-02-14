using Business.Abstract;
using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public sealed class UserDisplayNameResolver : IUserDisplayNameResolver
    {
        private readonly IUserService _userService;

        public UserDisplayNameResolver(IUserService userService)
        {
            _userService = userService;
        }
        public string GetDisplayName(int userId)
        {
            // TODO: DB'den getir
             return _userService.GetById(userId).Data.FirstName;
            //throw new NotImplementedException("Wire this to your user store.");
        }
    }

}

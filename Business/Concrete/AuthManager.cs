using Business.Abstract;
using Business.Security.Jwt;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private readonly IUserService _userService;
        private readonly JwtHelper _jwtHelper;

        public AuthManager(IUserService userService, JwtHelper jwtHelper)
        {
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        public IDataResult<User> Register(UserForRegisterDto dto, string password)
        {
            HashingHelper.CreatePasswordHash(password, out var hash, out var salt);
            var user = new User
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = true
            };
            _userService.Add(user);
            return new SuccessDataResult<User>(user, "User registered");
        }

        public IDataResult<User> Login(UserForLoginDto dto)
        {
            var user = _userService.GetByMail(dto.Email);
            if (user == null)
                return new ErrorDataResult<User>(null, "User not found");
            if (!HashingHelper.VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
                return new ErrorDataResult<User>(null, "Password is incorrect");

            return new SuccessDataResult<User>(user, "Login successful");
        }

        public IResult UserExists(string email)
            => _userService.GetByMail(email) != null
                ? new ErrorResult("User already exists")
                : new SuccessResult();

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = _userService.GetClaims(user);
            var token = _jwtHelper.CreateToken(user, claims);
            return new SuccessDataResult<AccessToken>(token);
        }
    }
}

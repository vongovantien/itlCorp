using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Common;
using API.Mobile.Models;

namespace API.Mobile.Repository
{
    public class UserRepositoryImpl : IUserRepository
    {
        private readonly User user = FakeData.user;

        public User Get(string userId)
        {
            User result = null;
            if(user.UserId == userId)
            {
                result = user;
            }
            return user;
        }
    }
}

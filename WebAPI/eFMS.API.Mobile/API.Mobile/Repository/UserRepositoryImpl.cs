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
            var user = FakeData.users.FirstOrDefault(x => x.UserId == userId);
            User result = null;
            if(user != null)
            {
                result = user;
            }
            return user;
        }
    }
}

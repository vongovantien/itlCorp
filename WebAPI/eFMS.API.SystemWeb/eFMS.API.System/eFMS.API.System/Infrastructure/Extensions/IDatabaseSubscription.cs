using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.Infrastructure.Extensions
{
    public interface IDatabaseSubscription
    {
        void Configure(string connectionString);
    }

}

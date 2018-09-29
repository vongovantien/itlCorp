using API.Mobile.Models;
using API.Mobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Repository
{
    public interface IJobRepository
    {
        JobViewModel Get(JobCriteria criteria, int? offset, int limit = 15);
        List<Job> GetBy(JobCriteria criteria);
    }
}

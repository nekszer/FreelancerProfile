using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProfile.DTO
{
    public class ClientReview
    {
        public double Rating { get; internal set; }
        public double PaidOut { get; internal set; }
        public string Comment { get; internal set; }
        public List<string> Skills { get; internal set; }
        public Profile Profile { get; internal set; }
    }
}

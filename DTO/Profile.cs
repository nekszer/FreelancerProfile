using System.Collections.Generic;

namespace FreelancerProfile.DTO
{
    public class Profile
    {
        public string UserName { get; internal set; }
        public string Name { get; internal set; }
        public string Letter { get; internal set; }
        public string Image { get; internal set; }
        public string Flag { get; internal set; }
        public double Rating { get; internal set; }
        public long RatingCount { get; internal set; }
        public double CostPerHour { get; internal set; }
        public List<Reputation> Reputation { get; internal set; }
        public List<ClientReview> Reviews { get; internal set; }
        public List<string> TopSkills { get; internal set; }
        public string InvitationLink { get; internal set; }
    }
}

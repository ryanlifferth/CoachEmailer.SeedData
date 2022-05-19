using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoachEmailer.SeedData.Models
{
    public class RawData
    {
        public string SchoolName { get; set; }
        public string SchoolNameShort { get; set; }
        public string CoachName { get; set; }
        public string CoachEmail { get; set; }
        public string CoachTitle { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Sport { get; set; }
        public string Division { get; set; }
        public string ConferenceName { get; set; }
        public string ConferenceNameShort { get; set; }
    }
}


/*{
    "SCHOOLNAME": "American University",
    "SCHOOLNAMESHORT": "American U",
    "COACHNAME": "Dan Louisignau",
    "COACHEMAIL": "danlou@american.edu",
    "COACHTITLE": "",
    "STATE": "Washington DC",
    "STATECODE": "DC",
    "SPORT": "M_SOCCER",
    "DIVISION": "NCAA_D1",
    "CONFERENCE": "",
    "CONFERENCENAMESHORT": ""
  }*/
using Microsoft.WindowsAzure.Storage.Table;

namespace CoachEmailer.SeedData.Models
{
    public class SchoolEntity : TableEntity
    {
        // PartitionKey:    School GUID (unique to each school)
        // RowKey:          [SPORT]_[DIVISION]
        //                  [SPORT]_[DIVISION]_STATE:[STATE]
        //                  [SPORT]_[DIVISION]_CONFERENCE:[CONFERENCE]  // TODO
        public string SchoolName { get; set; }
        public string SchoolNameLower { get; set; }
        public string SchoolNameShort { get; set; }
        public string SchoolNameShortLower { get; set; }
        public string StateCode { get; set; }
        public string State { get; set; }
        public string StateLower { get; set; }
        public string? ConferenceName { get; set; }
        public string? ConferenceNameLower { get; set; }
        public string? ConferenceNameShort { get; set; }
        public string? ConferenceNameShortLower { get; set; }
        public bool IsEnabled { get; set; }

    }
}


using Microsoft.WindowsAzure.Storage.Table;

namespace CoachEmailer.SeedData.Models
{
    public class CoachEntity : TableEntity
    {
        // PartitionKey:    School GUID (unique to each school)
        // RowKey:          EMAIL:[EmailAddress]
        //                  FIRSTNAME:[FirstName]   // TODO
        //                  LASTNAME:[LastName]     // TODO

        /// <summary>
        ///     Value from the RowKey
        /// </summary>
        public string Value { get; set; }

        public string CoachName { get; set; }
        public string? CoachTitle { get; set; }
        public string Email { get; set; }
        public string SchoolName { get; set; }
        public string SchoolNameShort { get; set; }
        public string Sport { get; set; }
        public string Division { get; set; }
        public string? ConferenceName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsBadEmail { get; set; }
        public string? Notes { get; set; }

    }
}

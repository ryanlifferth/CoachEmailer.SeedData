using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using CoachEmailer.SeedData.Models;


namespace CoachEmailer.SeedData
{
    class Program
    {

        static string _azureTableConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagecoachemailer;AccountKey=JY4KdRmzxZaHiDH+7BmfcaM1m7aWwguMyhWSapXzD1qjiZs9+VWGBqqvWoJo6kduZ5IljxhOKwBBQHfPaGbhRQ==;EndpointSuffix=core.windows.net";
        static private CloudTableClient? _cloudTableClient;
        static private CloudTable? _cloudTable;

        static void Main(string[] arg)
        {
            // Establish connection with Azure cloud storage
            Console.WriteLine("Setting up Azure access....");
            var storageAccount = CloudStorageAccount.Parse(_azureTableConnectionString);
            _cloudTableClient = storageAccount.CreateCloudTableClient();

            // Load data from JSON file
            Console.WriteLine("Reading JSON file...");
            var rawData = LoadDataFromJsonFile();

            // Format and save data to database
            Console.WriteLine("Importing into azure table...");
            SaveDataToAzureTables(rawData);

            Console.WriteLine("");
            Console.WriteLine("--------------------");
            Console.WriteLine("DONE.");
            Console.Read();
        }

        /// <summary>
        ///     Reads json data from file, then deserializes it into
        ///     a RawData type list.
        /// </summary>
        /// <returns>RawData type ordered by School name (need this for GUID creation)</returns>
        static IEnumerable<RawData> LoadDataFromJsonFile()
        {
            var path = "data//schools-coaches_20220518.json";
            string json = File.ReadAllText(path);

            var rawData = JsonConvert.DeserializeObject<IEnumerable<RawData>>(json)?.OrderBy(x => x.SchoolName);

            return rawData != null ? rawData : new List<RawData>();
        }

        /// <summary>
        ///     Formats the data the way it needs to be formatted for the database
        ///     Then save the database to the Azure Table Storage tables.
        ///     This method assumes that the RawData comes in sorted alphabetically
        ///     by school name, so that when we loop through the schools we only create
        ///     a new GUID (ID) once for each school.
        /// </summary>
        /// <param name="rawData"></param>
        static void SaveDataToAzureTables(IEnumerable<RawData> rawData)
        {
            string previousSchoolName = "";
            string schoolGuid = "";
            string rowKey = "";

            foreach (var item in rawData)
            {
                // Only create a new GUID if it is a new school
                if (previousSchoolName != item.SchoolName)
                {
                    // Create the new GUID for the school that will be its unique number
                    schoolGuid = Guid.NewGuid().ToString();
                    previousSchoolName = item.SchoolName;

                    // RowKey: [SPORT]_[DIVISION]
                    rowKey = $"{item.Sport}_{item.Division}";
                    var schoolEntity = CreateSchoolEntitiesFromData(schoolGuid, rowKey, item);

                    // RowKey: [SPORT]_[DIVISION]_state:[STATECODE]
                    rowKey = $"{item.Sport}_{item.Division}_state:{item.StateCode}";
                    var schoolEntityState = CreateSchoolEntitiesFromData(schoolGuid, rowKey, item);
                    SaveItemToAzure<SchoolEntity>(schoolEntity, "TESTSCHOOLS");
                    SaveItemToAzure<SchoolEntity>(schoolEntityState, "TESTSCHOOLS");
                    Console.WriteLine($"{item.SchoolNameShort} added to Azure TESTSCHOOLS table.");
                }

                var coachEntity = CreateCoachEntityFromData(schoolGuid, item);

                SaveItemToAzure<CoachEntity>(coachEntity, "TESTCOACHES");
                Console.WriteLine($"\t- {item.CoachName} added to Azure TESTCOACHES table.");
            }
        }

        /// <summary>
        ///     Builds a school entity from the raw data.  Formatted the way needed
        ///     for the Azure Table Storage tables.
        /// </summary>
        /// <param name="schoolGuid"></param>
        /// <param name="rowKey"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        static SchoolEntity CreateSchoolEntitiesFromData(
                string schoolGuid,
                string rowKey,
                RawData rawData)
        {
            var school = new SchoolEntity();
            school.PartitionKey = schoolGuid;
            school.RowKey = rowKey;
            school.SchoolName = rawData.SchoolName;
            school.SchoolNameLower = rawData.SchoolName.ToLower();
            school.SchoolNameShort = rawData.SchoolNameShort;
            school.SchoolNameShortLower = rawData.SchoolNameShort.ToLower();
            school.StateCode = rawData.StateCode;
            school.State = rawData.State;
            school.StateLower = rawData.State.ToLower();
            school.ConferenceName = string.IsNullOrEmpty(rawData.ConferenceName) ? null : rawData.ConferenceName;
            school.ConferenceNameLower = string.IsNullOrEmpty(rawData.ConferenceName) ? null : rawData.ConferenceName.ToLower();
            school.ConferenceNameShort = string.IsNullOrEmpty(rawData.ConferenceNameShort) ? null : rawData.ConferenceNameShort;
            school.ConferenceNameShortLower = string.IsNullOrEmpty(rawData.ConferenceNameShort) ? null : rawData.ConferenceNameShort.ToLower();
            school.IsEnabled = true;

            return school;
        }

        /// <summary>
        ///     Builds a Coach Entity from the raw data.  Formatted the way needed
        ///     for the Azure Table Storage tables.
        /// </summary>
        /// <param name="schoolGuid"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        static CoachEntity CreateCoachEntityFromData(string schoolGuid, RawData rawData)
        {
            var coach = new CoachEntity();
            coach.PartitionKey = schoolGuid;
            coach.RowKey = $"email:{rawData.CoachEmail}";
            coach.Value = rawData.CoachEmail;
            coach.CoachName = rawData.CoachName;
            coach.CoachTitle = String.IsNullOrEmpty(rawData.CoachTitle) ? null : rawData.CoachTitle;
            coach.Email = rawData.CoachEmail;
            coach.SchoolName = rawData.SchoolName;
            coach.SchoolNameShort = rawData.SchoolNameShort;
            coach.Sport = rawData.Sport;
            coach.Division = rawData.Division;
            coach.ConferenceName = String.IsNullOrEmpty(rawData.ConferenceName) ? null : rawData.ConferenceName;
            coach.IsEnabled = true;
            coach.IsBadEmail = false;
            coach.Notes = null;
            
            return coach;
        }

        /// <summary>
        ///     Saves data to the Azure Table storage tables in Azure cloud.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="cloudTable"></param>
        static void SaveItemToAzure<T>(T entity, string cloudTable) where T : TableEntity
        {
            _cloudTable = _cloudTableClient?.GetTableReference(cloudTable);

            var saveTask = Task.Run(async () => await _cloudTable.ExecuteAsync(TableOperation.InsertOrReplace(entity)));
            var result = saveTask.GetAwaiter().GetResult();

            //LogService.LogInformation($"Updated item: {JsonConvert.SerializeObject(entity)}");
        }

    }
}
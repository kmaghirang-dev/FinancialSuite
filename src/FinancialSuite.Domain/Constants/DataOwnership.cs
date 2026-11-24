namespace FinancialSuite.Domain.Constants
{
    public static class DataOwnership
    {
        // The owner account: the only user allowed to change records created by seeders and system syncs.
        public const string OwnerEmail = "kev.maghirang@gmail.com";

        // CreatedBy values written by seeders and background syncs; these records are visible to every user.
        public static readonly string[] SystemCreators = ["Seeder", "RiotSync", "GenshinSync", "StarRailSync"];
    }
}

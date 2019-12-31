namespace BillingProvidersApp.Core
{
    sealed public class Strings
    {
        // Static class
        private Strings() { }

        sealed public class SettingsBag
        {
            public const string RestoreFromDbError = "Settings are not available - the database is corrupted";
            public const string StoreToDbError = "Settings are not saved - the database is corrupted";
        }
        sealed public class BusinessObject
        {
            public const string LoadObjectError = "Can't load selected object - the database is corrupted";
            public const string LoadObjectsError = "Can't load objects - the database is corrupted";
            public const string SaveObjectError = "Values you've entered are either empty or invalid";
            public const string DeleteObjectError = "Can't delete object while there are references on it";
        }
    }
}

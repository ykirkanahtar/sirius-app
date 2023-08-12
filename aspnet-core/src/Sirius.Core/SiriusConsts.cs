using Sirius.Debugging;

namespace Sirius
{
    public class SiriusConsts
    {
        public const string LocalizationSourceName = "Sirius";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "350b347f898545b18a903416029f72b8";
    }
}

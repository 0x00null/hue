using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ZeroNull.Hue.StateStorage
{
    /// <summary>
    /// An App State Storage provider which stores data in a JSON file in the user's home directory
    /// </summary>
    public class OnDiskAppStateStore : IAppStateStore
    {
        private readonly string path;
        private static readonly JsonSerializerSettings serializerSettings = null;

        static OnDiskAppStateStore()
        {
            serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
            serializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy(), true));

        }

        public OnDiskAppStateStore()
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".hue",
                "state.json"
            );
        }


        public AppState Clear()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return new AppState();
        }

        public AppState Get()
        {
            if (!File.Exists(path))
            {
                return new AppState();
            }
            try
            {
                return JsonConvert.DeserializeObject<AppState>(File.ReadAllText(path), serializerSettings);
            }
            catch
            {
                return new AppState();
            }
        }

        public void Put(AppState state)
        {
            // Check the folder exists
            var folderPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(state, serializerSettings));
        }
    }
}

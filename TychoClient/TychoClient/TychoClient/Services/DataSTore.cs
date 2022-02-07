using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TychoClient.Services
{
    public class DataStore
    {
        private static DataStore _instance;
        private string cachePath = $"/storage/emulated/0/Android/data/com.companyname.TychoClient/files/usersCache.json";

        public static DataStore GetInstance()
        {
            if (_instance is null)
                _instance = new DataStore();
            return _instance;

        }

        private UserList _memoryCache;

        public UserList GetUserList()
        {
            this.Log("Getting User list...");

            if(_memoryCache != null)
            {
                this.Log("Memory cache hit. Taking list from memory cache.");
                return _memoryCache;
            }

            if (File.Exists(cachePath))
            {
                this.Log("File cache hit. Taking list from file cache.");
                UpdateCache(File.ReadAllText(cachePath));
                return _memoryCache;
            }
            else
            {
                this.Log("Cache miss. Loading from GitHub.");
                var json = FetchJsonAsync().Result;
                UpdateCache(json);
                return UserList.FromJson(json);
            }
        }
        
        public void ForceRefreshCache()
        {
            this.Log("Force refresh Users.");
            var task = Task.Run(() => {
                var json = FetchJsonAsync().Result;
                UpdateCache(json);
            });
        }

        private async Task<string> FetchJsonAsync()
        {
            this.Log("Fetching Users list from GitHub.");
            var client = new HttpClient();

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://raw.githubusercontent.com/Florian-Riesen/tycho-client/main/TychoData/users-db.json"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");

                    var message = await httpClient.SendAsync(request);

                    return await message.Content.ReadAsStringAsync();
                }
            }
        }

        private void UpdateCache(string json)
        {
            _memoryCache = UserList.FromJson(json);
            File.WriteAllText(cachePath, json);
        }
    }

    public class User
    {
        public Guid id { get; set; }
        public byte sid { get; set; }
        public string name { get; set; }
    }

    public class UserList : List<User>
    {
        public static UserList FromJson(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<UserList>(json);
    }
}

namespace Game.Robots
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public static class UriTools
    {
        public static async Task<T> LoadAsync<T>(string uri)
        {
            var text = await LoadStringAsync(uri);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static async Task PatchAsync(string uri, object target)
        {
            var text = await LoadStringAsync(uri);
            JsonConvert.PopulateObject(text, target);
        }

        public static Task<string> LoadStringAsync(string uri)
            => LoadStringAsync(new Uri(uri));

        public static async Task<string> LoadStringAsync(Uri uri)
        {
            string retVal = null;
            if (uri.Scheme == "file")
                retVal = await File.ReadAllTextAsync(uri.LocalPath);
            else
                using (var client = new WebClient())
                    retVal = await client.DownloadStringTaskAsync(uri);

            return retVal;
        }
    }
}

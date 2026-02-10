using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ValorantAccountManager.Core
{
    public static class ValorantApiRequest
    {
        public static async Task<string> GetMmrAsync(string name, string tag, string region)
        {
            using var client = new HttpClient();
            var url = $"https://vaccie.pythonanywhere.com/mmr/{name}/{tag}/{region}";
            return await client.GetStringAsync(url);
        }
    }
}

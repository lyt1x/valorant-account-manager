using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ValorantAccountManager.Core
{
    public static class ValorantApiRequest
    {
        public static async Task<string> GetMmrAsync(string name, string tag, string region)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            name = Uri.EscapeDataString(name);
            tag = Uri.EscapeDataString(tag);
            region = Uri.EscapeDataString(region);

            var url = $"https://vaccie.pythonanywhere.com/mmr/{name}/{tag}/{region}";
            return await client.GetStringAsync(url);
        }
    }
}

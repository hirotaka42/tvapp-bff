using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xiao.TVapp.Bff.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Xiao.TVapp.Bff.Controllers
{
    [Route("api/[controller]")]
    public class TVappController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        [HttpGet("session")]
        public async Task<IActionResult> GetSessionToken()
        {
            var requestContent = new StringContent("device_type=pc", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            // HttpRequestMessageを使用してリクエストごとにヘッダーを設定
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://platform-api.tver.jp/v2/api/platform_users/browser/create");
            requestMessage.Content = requestContent;
            requestMessage.Headers.Add("Origin", "https://s.tver.jp");
            requestMessage.Headers.Add("Referer", "https://s.tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to create sessionToken");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(content);

            var sessionToken = new SessionToken
            {
                PlatformUid = jsonResponse["result"]["platform_uid"].ToString(),
                PlatformToken = jsonResponse["result"]["platform_token"].ToString()
            };

            return Ok(sessionToken);
        }
    }
}


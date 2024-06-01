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
        private readonly ILogger<TVappController> _logger;

        public TVappController(ILogger<TVappController> logger)
        {
            _logger = logger;
        }

        [HttpPost("session")]
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

        [HttpGet("ranking/{genre}")]
        public async Task<IActionResult> GetRankingDetail(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
            {
                return BadRequest("Genre is required");
            }

            if (!Enum.TryParse(typeof(Genre), genre, true, out var genreEnum))
            {
                return BadRequest("Invalid genre");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://service-api.tver.jp/api/v1/callEpisodeRankingDetail/{genre}");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve ranking details");
            }
            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] string platformUid, [FromQuery] string platformToken)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(platformUid) || string.IsNullOrWhiteSpace(platformToken))
            {
                return BadRequest("Keyword, platformUid and platformToken are required");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://platform-api.tver.jp/service/api/v1/callKeywordSearch?platform_uid={platformUid}&platform_token={platformToken}&keyword={keyword}");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve search results");
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }
    }
}


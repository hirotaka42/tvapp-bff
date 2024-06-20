using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Xiao.TVapp.Bff.Contexts;
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

        [HttpGet("service/search")]
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

        [HttpGet("service/callHome")]
        public async Task<IActionResult> CallHome([FromQuery] string platformUid, [FromQuery] string platformToken)
        {
            if ( string.IsNullOrWhiteSpace(platformUid) || string.IsNullOrWhiteSpace(platformToken))
            {
                return BadRequest("platformUid and platformToken are required");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://platform-api.tver.jp/service/api/v1/callHome?platform_uid={platformUid}&platform_token={platformToken}");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve call results");
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        [HttpGet("service/callEpisode/{episodeId}")]
        public async Task<IActionResult> CallEpisode(string episodeId, [FromQuery] string platformUid, [FromQuery] string platformToken)
        {
            if (string.IsNullOrWhiteSpace(episodeId) || string.IsNullOrWhiteSpace(platformUid) || string.IsNullOrWhiteSpace(platformToken))
            {
                return BadRequest("episodeId, platformUid and platformToken are required");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://platform-api.tver.jp/service/api/v1/callEpisode/{episodeId}?platform_uid={platformUid}&platform_token={platformToken}");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve callEpisode results");
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        [HttpGet("content/episode/{episodeId}")]
        public async Task<IActionResult> CallEpisodeInfo(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
            {
                return BadRequest("episodeId are required");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://statics.tver.jp/content/episode/{episodeId}.json");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve content results");
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }

        [HttpGet("content/series/{seriesId}")]
        public async Task<IActionResult> CallSeriesInfo(string seriesId)
        {
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                return BadRequest("seriesId are required");
            }

            var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://statics.tver.jp/content/series/{seriesId}.json");

            requestMessage.Headers.Add("x-tver-platform-type", "web");
            requestMessage.Headers.Add("Origin", "https://tver.jp");
            requestMessage.Headers.Add("Referer", "https://tver.jp/");

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to retrieve content results");
            }

            var content = await response.Content.ReadAsStringAsync();

            return Ok(content);
        }


        [HttpGet("streaming/{episodeId}")]
        public async Task<IActionResult> GetStreamingUrl(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
            {
                return BadRequest("Episode ID is required");
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    // Todo;
                    // URLをダブルクォーテーションで囲むと失敗する(要改善)
                    Arguments = $"--get-url https://tver.jp/episodes/{episodeId}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // プロセスの開始と出力の取得
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // 非同期で標準出力/標準エラー出力から全てのデータを読み取り
                    string result = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    // プロセスが終了するまで待機
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError($"yt-dlp error: {error}");
                        return StatusCode(500, "Failed to retrieve streaming URL");
                    }

                    // 文字列の前後の空白を削除し、正確なURLを取得
                    return Ok(result.Trim());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving streaming URL");
                return StatusCode(500, "Internal server error");
            }
        }

        // [HttpGet("streaming/{episodeId}")]
        // public async Task<IActionResult> GetStreamingUrl(string episodeId)
        // {
        //     if (string.IsNullOrWhiteSpace(episodeId))
        //     {
        //         return BadRequest("Episode ID is required");
        //     }

        //     // DBからepisodeIdを検索
        //     var episode = await _context.Episodes.FindAsync(episodeId);

        //     if (episode != null)
        //     {
        //         // episodeIdが存在すれば、そのstreamingURLを返却
        //         return Ok(episode.StreamingUrl);
        //     }
        //     else
        //     {
        //         try
        //         {
        //             var startInfo = new ProcessStartInfo
        //             {
        //                 FileName = "yt-dlp",
        //                 Arguments = $"--get-url https://tver.jp/episodes/{episodeId}",
        //                 RedirectStandardOutput = true,
        //                 RedirectStandardError = true,
        //                 UseShellExecute = false,
        //                 CreateNoWindow = true
        //             };

        //             // プロセスの開始と出力の取得
        //             using (var process = new Process { StartInfo = startInfo })
        //             {
        //                 process.Start();

        //                 // 非同期で標準出力/標準エラー出力から全てのデータを読み取り
        //                 string result = await process.StandardOutput.ReadToEndAsync();
        //                 string error = await process.StandardError.ReadToEndAsync();

        //                 // プロセスが終了するまで待機
        //                 process.WaitForExit();

        //                 if (process.ExitCode != 0)
        //                 {
        //                     _logger.LogError($"yt-dlp error: {error}");
        //                     return StatusCode(500, "Failed to retrieve streaming URL");
        //                 }

        //                 // 新しいエピソードを作成し、DBに保存
        //                 var newEpisode = new Episode { Id = episodeId, StreamingUrl = result.Trim() };
        //                 _context.Episodes.Add(newEpisode);
        //                 await _context.SaveChangesAsync();

        //                 // streamingURLを返却
        //                 return Ok(newEpisode.StreamingUrl);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             _logger.LogError(ex, "Exception occurred while retrieving streaming URL");
        //             return StatusCode(500, "Internal server error");
        //         }
        //     }
        // }

    }
}


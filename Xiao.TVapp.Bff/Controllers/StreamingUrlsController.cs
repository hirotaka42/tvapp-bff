using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xiao.TVapp.Bff.Contexts;
using Xiao.TVapp.Bff.Models;

namespace Xiao.TVapp.Bff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamingUrlsController : ControllerBase
    {
        private readonly StreamingUrlsContext _context;
        private readonly ILogger<StreamingUrlsController> _logger;

        public StreamingUrlsController(StreamingUrlsContext context, ILogger<StreamingUrlsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/StreamingUrls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StreamingUrls>>> GetStreamingUrls()
        {
          if (_context.StreamingUrls == null)
          {
              return NotFound();
          }
            return await _context.StreamingUrls.ToListAsync();
        }

        // GET: api/StreamingUrls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StreamingUrls>> GetStreamingUrlsById(int id)
        {
          if (_context.StreamingUrls == null)
          {
              return NotFound();
          }
            var streamingUrls = await _context.StreamingUrls.FindAsync(id);

            if (streamingUrls == null)
            {
                return NotFound();
            }

            return streamingUrls;
        }

        // GET: api/StreamingUrls/episode/epdkg43t
        [HttpGet("episode/{episodeId}")]
        public async Task<IActionResult> GetStreamingUrlsByEpisodeId(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
            {
                return BadRequest("Episode ID is required");
            }

            // DBからepisodeIdを検索
            var episode = await _context.StreamingUrls
                .FirstOrDefaultAsync(s =>s.EpisodeId == episodeId);

            if (episode != null)
            {
                // 現在の日時とUpdatedAtの差が2日以上ならば、URLを取得し直す
                if ((DateTime.UtcNow - episode.UpdatedAt).TotalDays >= 2)
                {
                    episode.StreamingUrl = await GetStreamingUrlFromYtDlp(episodeId);
                    episode.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return Ok(episode.StreamingUrl);
            }
            else
            {
                try
                {
                    var newStreamingUrl = await GetStreamingUrlFromYtDlp(episodeId);
                    DateTime dateTime = DateTime.UtcNow;

                    // 新しいエピソードを作成し、DBに保存
                    StreamingUrls newEpisode = new()
                    {
                        EpisodeId = episodeId,
                        StreamingUrl = newStreamingUrl,
                        CreatedAt = dateTime,
                        UpdatedAt = dateTime,
                    };
                    _context.StreamingUrls.Add(newEpisode);
                    await _context.SaveChangesAsync();

                    return Ok(newEpisode.StreamingUrl);
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while retrieving streaming URL");
                    return StatusCode(500, "Internal server error");
                }
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStreamingUrls(int id, StreamingUrls streamingUrls)
        {
            if (id != streamingUrls.Id)
            {
                return BadRequest();
            }

            _context.Entry(streamingUrls).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StreamingUrlsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST: api/StreamingUrls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StreamingUrls>> PostStreamingUrls(StreamingUrls streamingUrls)
        {
          if (_context.StreamingUrls == null)
          {
              return Problem("Entity set 'StreamingUrlsContext.StreamingUrls'  is null.");
          }
            _context.StreamingUrls.Add(streamingUrls);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStreamingUrls", new { id = streamingUrls.Id }, streamingUrls);
        }

        // DELETE: api/StreamingUrls/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStreamingUrls(int id)
        {
            if (_context.StreamingUrls == null)
            {
                return NotFound();
            }
            var streamingUrls = await _context.StreamingUrls.FindAsync(id);
            if (streamingUrls == null)
            {
                return NotFound();
            }

            _context.StreamingUrls.Remove(streamingUrls);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StreamingUrlsExists(int id)
        {
            return (_context.StreamingUrls?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task<string> GetStreamingUrlFromYtDlp(string episodeId)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
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
                    throw new Exception("Failed to retrieve streaming URL");
                }

                return result.Trim();
            }
        }
    }
}

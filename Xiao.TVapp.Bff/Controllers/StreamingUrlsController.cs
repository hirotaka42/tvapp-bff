using System;
using System.Collections.Generic;
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

        public StreamingUrlsController(StreamingUrlsContext context)
        {
            _context = context;
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

        // GET: api/StreamingUrls/episode/{episodeId}
        [HttpGet("episode/{episodeId}")]
        public async Task<ActionResult<StreamingUrls>> GetStreamingUrlsByEpisodeId(string episodeId)
        {
            if (_context.StreamingUrls == null)
            {
                return NotFound();
            }

            var streamingUrls = await _context.StreamingUrls
                .FirstOrDefaultAsync(s => s.EpisodeId == episodeId);

            if (streamingUrls == null)
            {
                return NotFound();
            }

            return streamingUrls;
        }

        // PUT: api/StreamingUrls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
    }
}

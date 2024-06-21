using System;
namespace Xiao.TVapp.Bff.Models
{
	public class StreamingUrls
	{
        public int Id { get; set; }
        public string EpisodeId { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string StreamingUrl { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}


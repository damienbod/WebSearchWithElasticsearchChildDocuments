using System;
using System.ComponentModel.DataAnnotations;

namespace WebSearchWithElasticsearchChildDocuments.SearchEngine
{
	public class CountryRegion
	{
		[Key]
		[StringLength(3)]
		public string CountryRegionCode { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		public DateTime ModifiedDate { get; set; }
	}
}
using System;
using System.ComponentModel.DataAnnotations;
using ElasticsearchCRUD.ContextAddDeleteUpdate.CoreTypeAttributes;

namespace WebSearchWithElasticsearchChildDocuments.SearchEngine
{
	public class StateProvince
	{
		[Key]
		public int StateProvinceID { get; set; }

		[Required]
		[StringLength(3)]
		public string StateProvinceCode { get; set; }

		[Required]
		[StringLength(3)]
		public string CountryRegionCode { get; set; }

		[ElasticsearchBoolean]
		public bool IsOnlyStateProvinceFlag { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		public int TerritoryID { get; set; }

		public Guid rowguid { get; set; }

		public DateTime ModifiedDate { get; set; }
	}
}
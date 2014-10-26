using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ElasticsearchCRUD;
using WebSearchWithElasticsearchChildDocuments.Models;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public class ElasticSearchProvider : ISearchProvider, IDisposable
	{
		private const string ConnectionString = "http://localhost:9200/";
		private readonly IElasticSearchMappingResolver _elasticSearchMappingResolver;
		private readonly ElasticSearchContext _context;

		public ElasticSearchProvider()
		{
			_elasticSearchMappingResolver = new ElasticSearchMappingResolver();
			_elasticSearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Address), new ElasticSearchMappingAddress());
		    _context = new ElasticSearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticSearchMappingResolver,true,true));
		}

		public IEnumerable<T> QueryString<T>(string term) 
		{ 
			return _context.Search<T>(BuildQueryStringSearch(term)).PayloadResult.ToList();
		}

		private string BuildQueryStringSearch(string term)
		{
			var names = "";
			if (term != null)
			{
				names = term.Replace("+", " OR *");
			}

			var buildJson = new StringBuilder();
			buildJson.AppendLine("{");
			buildJson.AppendLine(" \"query\": {");
			buildJson.AppendLine("   \"query_string\": {");
			buildJson.AppendLine("      \"query\": \"" + names + "*\"");
			buildJson.AppendLine("     }");
			buildJson.AppendLine("  }");
			buildJson.AppendLine("}");

			return buildJson.ToString();
		}

		public void AddUpdateDocument(Address address)
		{
			_context.AddUpdateDocument(address, address.AddressID, address.StateProvinceID);
			_context.SaveChanges();
		}

		public void UpdateAddresses(long stateProvinceId, List<Address> addresses)
		{
			foreach (var item in addresses)
			{
				_context.AddUpdateDocument(item, item.AddressID, item.StateProvinceID);
			}

			_context.SaveChanges();
		}

		[HttpPost]
		public void DeleteAddress(long addressId)
		{
			_context.DeleteDocument<Address>(addressId);
			_context.SaveChanges();
		}

		public List<SelectListItem> GetAllStateProvinces()
		{
			var result = from element in _context.Search<StateProvince>("").PayloadResult
						 select new SelectListItem
						 {
							 Text = string.Format("StateProvince: {0}, CountryRegionCode {1}", 
							 element.StateProvinceCode, element.CountryRegionCode), 
							 Value = element.StateProvinceID.ToString()
						 };

			return result.ToList();
		}

		public PagingTableResult<Address> GetAllAddressesForStateProvince(string stateprovinceid, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var result = new PagingTableResult<Address>();
			var data = _context.Search<Address>(
							BuildSearchForChildDocumentsWithIdAndParentType(
								stateprovinceid, 
								"stateprovince",
								jtStartIndex, 
								jtPageSize, 
								jtSorting)
						);

			result.Items = data.PayloadResult.ToList();
			result.TotalCount = data.TotalHits;
			return result;
		}

		// {
		//  "from": 0, "size": 10,
		//  "query": {
		//	"term": { "_parent": "parentdocument#7" }
		//  }
		// }
		private string BuildSearchForChildDocumentsWithIdAndParentType(object parentId, string parentType, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var buildJson = new StringBuilder();
			buildJson.AppendLine("{");
			buildJson.AppendLine("\"from\" : " + jtStartIndex + ", \"size\" : " + jtPageSize + ",");
			buildJson.AppendLine("\"query\": {");
			buildJson.AppendLine("\"term\": {\"_parent\": \"" + parentType + "#" + parentId + "\"}");
			buildJson.AppendLine("}");
			buildJson.AppendLine("}");

			return buildJson.ToString();
		}

		private bool isDisposed;
		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;
				_context.Dispose();
			}
		}
	}
}
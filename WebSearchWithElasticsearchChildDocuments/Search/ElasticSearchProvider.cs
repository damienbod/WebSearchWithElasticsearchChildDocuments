using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ElasticsearchCRUD;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public class ElasticSearchProvider : ISearchProvider
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
			return _context.Search<T>(BuildQueryStringSearch(term)).ToList();
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
			//var addressDatabase = context.SearchForChildDocumentsByParentId<Address>(stateProvinceId, typeof(StateProvince));
			foreach (var item in addresses)
			{
				// TODO delete if exists (Not yet supported)
				_context.AddUpdateDocument(item, item.AddressID, item.StateProvinceID);
			}

			_context.SaveChanges();
		}

		public void DeleteAddress(long deleteId)
		{
			_context.DeleteDocument<Address>(deleteId);
			_context.SaveChanges();
		}

		public List<SelectListItem> GetAllStateProvinces()
		{
			var result = from element in _context.Search<StateProvince>("")
						 select new SelectListItem { Text = string.Format("StateProvince: {0}, CountryRegionCode {1}", element.StateProvinceCode, element.CountryRegionCode), Value = element.StateProvinceID.ToString() };

			return result.ToList();
		}

		public List<Address> GetAllAddressesForStateProvince(string stateprovinceid)
		{
			return _context.SearchForChildDocumentsByParentId<Address>(stateprovinceid, typeof(StateProvince)).ToList();
		}
	}
}
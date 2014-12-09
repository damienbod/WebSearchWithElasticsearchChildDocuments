﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel;
using WebSearchWithElasticsearchChildDocuments.Models;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public class ElasticSearchProvider : ISearchProvider, IDisposable
	{
		private const string ConnectionString = "http://localhost:9200/";
		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver;
		private readonly ElasticsearchContext _context;

		public ElasticSearchProvider()
		{
			_elasticsearchMappingResolver = new ElasticsearchMappingResolver();
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Address), new ElasticsearchMappingAddress());
		    _context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver,true,true));
		}

		public IEnumerable<T> QueryString<T>(string term) 
		{ 
			return _context.Search<T>(BuildQueryStringSearch(term)).PayloadResult.Hits.HitsResult.Select(t =>t.Source).ToList();
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
			// if the parent has changed, the child needs to be deleted and created again. This in not required in this example
			_context.AddUpdateDocument(address, address.AddressID, new RoutingDefinition { ParentId = address.StateProvinceID });
			_context.SaveChanges();
		}

		public void UpdateAddresses(long stateProvinceId, List<Address> addresses)
		{
			foreach (var item in addresses)
			{
				// if the parent has changed, the child needs to be deleted and created again. This in not required in this example
				_context.AddUpdateDocument(item, item.AddressID, new RoutingDefinition{ParentId = item.StateProvinceID});
			}

			_context.SaveChanges();
		}

		public void DeleteAddress(long addressId)
		{
			_context.DeleteDocument<Address>(addressId);
			_context.SaveChanges();
		}

		public List<SelectListItem> GetAllStateProvinces()
		{
			var result = from element in _context.Search<StateProvince>("").PayloadResult.Hits.HitsResult.Select(t => t.Source)
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

			result.Items = data.PayloadResult.Hits.HitsResult.Select(t =>t.Source).ToList();
			result.TotalCount = data.PayloadResult.Hits.Total;
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
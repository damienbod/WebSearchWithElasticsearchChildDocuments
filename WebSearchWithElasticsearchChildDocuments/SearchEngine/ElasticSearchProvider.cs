using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Model.SearchModel.Sorting;
using WebSearchWithElasticsearchChildDocuments.Models;

namespace WebSearchWithElasticsearchChildDocuments.SearchEngine
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

		private Search BuildQueryStringSearch(string term)
		{
			var names = "";
			if (term != null)
			{
				names = term.Replace("+", " OR *");
			}
			var search = new Search
			{
				Query = new Query(new QueryStringQuery(names + "*"))
			};
			return search;
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

		public void DeleteAddress(long addressId, long selectedstateprovinceid)
		{
			_context.DeleteDocument<Address>(addressId, new RoutingDefinition {ParentId = selectedstateprovinceid});
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
		//  "term": { "_parent": "parentdocument#7" }
		//  },
		//  "sort": { "city" : { "order": "desc" } }"
		// }
		private Search BuildSearchForChildDocumentsWithIdAndParentType(object parentId, string parentType, int jtStartIndex, int jtPageSize, string jtSorting)
		{
			var search = new Search
			{
				From = jtStartIndex,
				Size = jtPageSize,
				Query = new Query(new TermQuery("_parent", parentType + "#" + parentId))
			};

			var sorts = jtSorting.Split(' ');
			if (sorts.Length == 2)
			{
				var order = OrderEnum.asc;
				if (sorts[1].ToLower() == "desc")
				{
					order = OrderEnum.desc;
				}

				search.Sort = CreateSortQuery(sorts[0].ToLower(), order);
			}
			return search;
		}

		public SortHolder CreateSortQuery(string sort, OrderEnum order)
		{
			return new SortHolder(
				new List<ISort>
				{
					new SortStandard(sort)
					{
						Order = order
					}
				}
			);
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
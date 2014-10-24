using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ElasticsearchCRUD;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public class ElasticSearchProvider : ISearchProvider
	{
		private const string ConnectionString = "http://localhost:9200/";
		private readonly IElasticSearchMappingResolver _elasticSearchMappingResolver = new ElasticSearchMappingResolver();

		private static readonly Uri Node = new Uri(ConnectionString);

		public IEnumerable<Address> QueryString(string term)
		{
			throw new NotImplementedException();
		}

		public void AddUpdateDocument(Address address)
		{
			using (var context = new ElasticSearchContext(ConnectionString, _elasticSearchMappingResolver))
			{
				context.AddUpdateDocument(address, address.AddressID, address.StateProvinceID);
				context.SaveChanges();
			}
		}

		public void UpdateAddresses(long stateProvinceId, List<Address> addresses)
		{
			using (var context = new ElasticSearchContext(ConnectionString, _elasticSearchMappingResolver))
			{
				//var addressDatabase = context.SearchForChildDocumentsByParentId<Address>(stateProvinceId, typeof(StateProvince));
				foreach (var item in addresses)
				{
					// TODO delete if exists (Not yet supported)
					context.AddUpdateDocument(item, item.AddressID, item.StateProvinceID);
				}
				
				context.SaveChanges();
			}
		}

		public void DeleteAddress(long deleteId)
		{
			using (var context = new ElasticSearchContext(ConnectionString, _elasticSearchMappingResolver))
			{
				context.DeleteDocument<Address>(deleteId);
				context.SaveChanges();
			}
		}
	}
}
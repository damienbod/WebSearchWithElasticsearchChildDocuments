using System.Collections.Generic;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public interface ISearchProvider
	{
		IEnumerable<Address> QueryString(string term);

		void AddUpdateDocument(Address address);
		void UpdateAddresses(long stateProvinceId, List<Address> addresses);
		void DeleteAddress(long updateId);
	}
}
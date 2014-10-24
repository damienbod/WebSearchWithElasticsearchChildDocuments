using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Mvc;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public interface ISearchProvider
	{
		IEnumerable<T> QueryString<T>(string term);

		void AddUpdateDocument(Address address);
		void UpdateAddresses(long stateProvinceId, List<Address> addresses);
		void DeleteAddress(long addressId);
		List<SelectListItem> GetAllStateProvinces();
		List<Address> GetAllAddressesForStateProvince(string stateprovinceid);
	}
}
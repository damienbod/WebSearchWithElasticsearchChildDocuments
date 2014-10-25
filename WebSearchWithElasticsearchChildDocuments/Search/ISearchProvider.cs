﻿using System.Collections.Generic;
using System.Web.Mvc;
using WebSearchWithElasticsearchChildDocuments.Models;

namespace WebSearchWithElasticsearchChildDocuments.Search
{
	public interface ISearchProvider
	{
		IEnumerable<T> QueryString<T>(string term);

		void AddUpdateDocument(Address address);
		void UpdateAddresses(long stateProvinceId, List<Address> addresses);
		void DeleteAddress(long addressId);
		List<SelectListItem> GetAllStateProvinces();
		PagingTableResult<Address> GetAllAddressesForStateProvince(string stateprovinceid, int jtStartIndex, int jtPageSize, string jtSorting);
	}
}
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebSearchWithElasticsearchChildDocuments.Search;

namespace WebSearchWithElasticsearchChildDocuments.Controllers
{
	[RoutePrefix("Search")]
	public class SearchController : Controller
	{
		readonly ISearchProvider _searchProvider = new ElasticSearchProvider();

		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[Route("Search")]
		public JsonResult Search(string term)
		{
			return Json(_searchProvider.QueryString<StateProvince>(term), "AddressListForStateProvince", JsonRequestBehavior.AllowGet);
		}

		[Route("GetAddressForStateProvince")]
		public JsonResult GetAddressForStateProvince(string stateprovinceid)
		{
			try
			{
				List<Address> addresses = _searchProvider.GetAllAddressesForStateProvince(stateprovinceid);
				return Json(new { Result = "OK", Records = addresses });
			}
			catch (Exception ex)
			{
				return Json(new { Result = "ERROR", Message = ex.Message });
			}
		}

		[Route("CreateAddressForStateProvince")]
		public JsonResult CreateAddressForStateProvince(Address address)
		{
			try
			{
				_searchProvider.AddUpdateDocument(address);
				return Json(new { Result = "OK", Records = address });
			}
			catch (Exception ex)
			{
				return Json(new { Result = "ERROR", Message = ex.Message });
			}
		}

		[HttpPost]
		[Route("DeleteAddress")]
		public ActionResult DeleteAddress(long addressId)
		{
			_searchProvider.DeleteAddress(addressId);
			return Json(new { Result = "OK"});
		}
	}
}
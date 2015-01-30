using System;
using System.Web.Mvc;
using WebSearchWithElasticsearchChildDocuments.SearchEngine;

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
		public JsonResult GetAddressForStateProvince(string stateprovinceid, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
		{
			try
			{
				var data = _searchProvider.GetAllAddressesForStateProvince(stateprovinceid, jtStartIndex, jtPageSize, jtSorting);
				return Json(new { Result = "OK", Records = data.Items, TotalRecordCount = data.TotalCount });
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
				address.ModifiedDate = DateTime.UtcNow;
				address.rowguid = Guid.NewGuid();
				_searchProvider.AddUpdateDocument(address);
				return Json(new { Result = "OK", Record = address });
			}
			catch (Exception ex)
			{
				return Json(new { Result = "ERROR", Message = ex.Message });
			}
		}

		[HttpPost]
		[Route("DeleteAddress")]
		public ActionResult DeleteAddress(long addressId, long selectedstateprovinceid)
		{
			_searchProvider.DeleteAddress(addressId, selectedstateprovinceid);
			return Json(new { Result = "OK"});
		}
	}
}
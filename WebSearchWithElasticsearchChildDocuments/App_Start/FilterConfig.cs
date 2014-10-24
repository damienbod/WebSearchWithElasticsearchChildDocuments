using System.Web;
using System.Web.Mvc;

namespace WebSearchWithElasticsearchChildDocuments
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}

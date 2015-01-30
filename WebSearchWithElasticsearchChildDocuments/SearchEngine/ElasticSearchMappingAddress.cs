﻿using System;
using ElasticsearchCRUD;

namespace WebSearchWithElasticsearchChildDocuments.SearchEngine
{
	public class ElasticsearchMappingAddress : ElasticsearchMapping
	{
		// This address type is a child type form stateprovince in the stateprovinces index
		public override string GetIndexForType(Type type)
		{
			return "stateprovinces";
		}
	}
}
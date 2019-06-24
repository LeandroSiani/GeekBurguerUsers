
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurguer.Users.Extensions
{
	public static class RequestExtensions
	{
		private static Uri GetUri(this Microsoft.AspNetCore.Http.HttpRequest request)
		{
			var builder = new UriBuilder();
			builder.Scheme = request.Scheme;
			builder.Host = request.Host.Value;
			builder.Path = request.Path;
			builder.Query = request.QueryString.ToUriComponent();
			return builder.Uri;
		}
	}
}

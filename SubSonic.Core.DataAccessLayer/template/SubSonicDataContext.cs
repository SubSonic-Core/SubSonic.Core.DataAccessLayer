using Microsoft.Extensions.DependencyInjection;
using SubSonic;
using System;

namespace TemplateRootNamespace
{
	namespace Models
	{
	#region Generate Database Models
	#endregion
	}

	public partial class TemplateSafeRootName
		: SubSonicContext
	{
		private readonly IServiceCollection services;

        public TemplateSafeRootName(IServiceCollection services)
		{
			this.services = services ?? throw new ArgumentNullException(nameof(services));
		}

		#region ISubSonicSetCollection{TEntity} Collection Properties
		#endregion
	}
}
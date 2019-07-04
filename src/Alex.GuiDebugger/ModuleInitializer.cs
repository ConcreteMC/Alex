using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Fonts;
using Alex.GuiDebugger.Services;
using Catel.IoC;
using Orchestra.Services;


/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
	/// <summary>
	/// Initializes the module.
	/// </summary>
	public static void Initialize()
	{
		var serviceLocator = ServiceLocator.Default;
		//serviceLocator.AutoRegisterTypesViaAttributes = true;

		serviceLocator.RegisterType<IRibbonService, RibbonService>();
		serviceLocator.RegisterType<IApplicationInitializationService, ApplicationInitializationService>();

		//serviceLocator.RegisterTypesUsingDefaultNamingConvention();

		FontMaterial.Initialize(false);
		FontAwesome.Initialize();
	}
}
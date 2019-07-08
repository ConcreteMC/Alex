using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Models;
using Alex.GuiDebugger.ViewModels;
using Alex.GuiDebugger.ViewModels.Documents;
using Alex.GuiDebugger.ViewModels.Tools;
using Alex.GuiDebugger.Views;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Editor;
using Dock.Model;
using Dock.Model.Controls;

namespace Alex.GuiDebugger.Factories
{
	public class DefaultDockFactory : DockFactory
	{
		private object _context;

		public DefaultDockFactory(object context)
		{
			_context = context;
		}

		public override IDock CreateLayout()
		{
			var elementTreeDocument = new ElementTreeDocument
			{
				Id    = "ElementTreeDocument",
				Title = "ElementTreeDocument"
			};

			var elementTreeTool = new ElementTreeTool
			{
				Id    = "ElementTreeTool",
				Title = "ElementTreeTool"
			};

			var mainLayout = new LayoutDock
			{
				Id          = "MainLayout",
				Title       = "MainLayout",
				Proportion  = double.NaN,
				Orientation = Orientation.Horizontal,
				CurrentView = null,
				Views = CreateList<IView>(new LayoutDock()
										  {
											  Id          = "LeftPane",
											  Title       = "LeftPane",
											  Proportion  = double.NaN,
											  Orientation = Orientation.Vertical,
											  CurrentView = null,
											  Views = CreateList<IView>(new ToolDock()
											  {
												  Id          = "LeftPaneTop",
												  Title       = "LeftPaneTop",
												  Proportion  = double.NaN,
												  CurrentView = elementTreeTool,
												  Views       = CreateList<IView>(elementTreeTool)
											  })
										  },
										  new SplitterDock()
										  {
											  Id    = "LeftSplitter",
											  Title = "LeftSplitter"
										  },
										  new DocumentDock()
										  {
											  Id         = "DocumentsPane",
											  Title      = "DocumentsPane",
											  Proportion = double.NaN,
											  CurrentView = elementTreeDocument,
											  Views = CreateList<IView>(elementTreeDocument)
										  })
			};

			var mainView = new MainView
			{
				Id          = "Main",
				Title       = "Main",
				CurrentView = mainLayout,
				Views       = CreateList<IView>(mainLayout)
			};

			var root = CreateRootDock();

			root.Id    = "Root";
			root.Title = "Root";
			root.CurrentView    = mainView;
			root.DefaultView    = mainView;
			root.Views          = CreateList<IView>(mainView);
			root.Left           = CreatePinDock();
			root.Left.Alignment = Alignment.Left;

			return root;
		}
		public override void InitLayout(IView layout)
		{
			this.ContextLocator = new Dictionary<string, Func<object>>
			{
				[nameof(IRootDock)] = () => _context,
				[nameof(IPinDock)] = () => _context,
				[nameof(ILayoutDock)] = () => _context,
				[nameof(IDocumentDock)] = () => _context,
				[nameof(IToolDock)] = () => _context,
				[nameof(ISplitterDock)] = () => _context,
				[nameof(IDockWindow)] = () => _context,
				[nameof(IDocumentTab)] = () => _context,
				[nameof(IToolTab)] = () => _context,
				["ElementTreeDocument"] = () => new ElementTreeDocumentModel(),
				["ElementTreeTool"] = () => new ElementTreeToolModel(),
				["LeftPane"] = () => _context,
				["LeftPaneTop"] = () => _context,
				["ElementTreeTool"] = () => _context,
				["DocumentsPane"] = () => _context,
				["MainLayout"] = () => _context,
				["LeftSplitter"] = () => _context,
				["MainLayout"] = () => _context,
				["Main"] = () => layout,
				["Editor"] = () => new LayoutEditor()
				{
					Layout = layout
				}
			};

			this.HostLocator = new Dictionary<string, Func<IDockHost>>
			{
				[nameof(IDockWindow)] = () => new HostWindow()
			};

			base.InitLayout(layout);
		}
	}
}
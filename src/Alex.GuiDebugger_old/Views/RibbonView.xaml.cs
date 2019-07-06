using Orchestra;

namespace Alex.GuiDebugger.Views
{
    using Catel.Windows.Controls;

    /// <summary>
    /// Interaction logic for RibbonView.xaml.
    /// </summary>
    public partial class RibbonView : UserControl
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonView"/> class.
        /// </summary>
        public RibbonView()
        {
            InitializeComponent();

            ribbon.AddAboutButton();
        }
        #endregion

        #region Methods
        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();

#pragma warning disable WPF0041
            //backstageTabControl.DataContext = ViewModel;
#pragma warning restore WPF0041
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alex.Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            PlayButton.Command = PlayAlexCommand;
        }

        public ICommand PlayAlexCommand
        {
            get { return new SimpleCommand(o => Program.RunAlex()); }
        }
    }

    public class SimpleCommand : ICommand
    {
        private Func<object, bool> _predicate;
        private Action<object> _action;

        public SimpleCommand(Func<object, bool> predicate, Action<object> action)
        {
            _predicate = predicate;
            _action = action;
        }
        public SimpleCommand(Action<object> action) : this(null, action)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _predicate?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}

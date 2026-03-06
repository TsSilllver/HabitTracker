using HabitTracker.Core.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HabitTracker.Core.Models;


namespace HabitTracker.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var habits = new List<Habit>
            {
            new Habit { Name = "Встать в 6 утра", TargetValue = 1, Unit = "раз" },
            new Habit { Name = "Подтягивания", TargetValue = 30, Unit = "раз" },
            new Habit { Name = "Вода", TargetValue = 4, Unit = "литра" }
             };
            HabitsListBox.ItemsSource = habits;
            HabitsListBox.DisplayMemberPath = "Name"; 
        }
    }
}
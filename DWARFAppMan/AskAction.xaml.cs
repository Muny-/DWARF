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

namespace DWARFAppMan
{
    /// <summary>
    /// Interaction logic for AskAction.xaml
    /// </summary>
    public partial class AskAction : Elysium.Controls.Window
    {
        public string Action;
        public string Type;
        public string ItemName;
        public string Permissions;

        public AskAction(string Action, string Type, string ItemName, string permLabelT, string Permissions)
        {
            this.ItemName = ItemName;
            this.Permissions = Permissions;
            this.Action = Action;
            this.Type = Type;
            InitializeComponent();
            appNameText.Text = ItemName;
            permissionsText.Text = Permissions;
            permLabel.Content = permLabelT;
            actionButton.Content = Action;
            this.Title = "DWARF - " + Action + " " + Type;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

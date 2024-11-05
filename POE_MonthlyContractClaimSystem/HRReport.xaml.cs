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
using System.Windows.Shapes;

namespace POE_MonthlyContractClaimSystem
{
    public partial class HRReport : Window
    {
        public HRReport()
        {
            InitializeComponent();
            LoadApprovedClaims();
        }

        private void LoadApprovedClaims()
        {
            using (var context = new AppDbContext())
            {
                // Load only approved claims
                var approvedClaims = context.Claims.Where(c => c.Status == "Approved").ToList();
                ApprovedClaimsGrid.ItemsSource = approvedClaims; // Bind to DataGrid
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}


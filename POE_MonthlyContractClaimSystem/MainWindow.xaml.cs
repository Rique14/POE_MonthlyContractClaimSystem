using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace POE_MonthlyContractClaimSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPendingClaims();
        }

        private void CalculateTotal(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(HoursWorked.Text, out double hours) &&
                double.TryParse(HourlyRate.Text, out double rate))
            {
                TotalPayment.Text = (hours * rate).ToString("C");
            }
            else
            {
                TotalPayment.Text = "0.00";
            }
        }

        private void SubmitClaim(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LecturerName.Text) ||
                string.IsNullOrEmpty(HoursWorked.Text) ||
                string.IsNullOrEmpty(HourlyRate.Text) ||
                string.IsNullOrEmpty(UploadedFileName.Text))
            {
                MessageBox.Show("Please fill in all required fields and upload a document.");
                return;
            }

            try
            {
                var hoursWorked = Convert.ToDouble(HoursWorked.Text);
                var hourlyRate = Convert.ToDouble(HourlyRate.Text);
                var totalAmount = hoursWorked * hourlyRate;

                var claim = new Claim
                {
                    LecturerName = LecturerName.Text,
                    HoursWorked = hoursWorked,
                    HourlyRate = hourlyRate,
                    TotalAmount = totalAmount,
                    Notes = AdditionalNotes.Text,
                    Status = "Pending",
                    DocumentPath = UploadedFileName.Text
                };

                using (var context = new AppDbContext())
                {
                    context.Claims.Add(claim);
                    context.SaveChanges();
                }

                MessageBox.Show("Claim Submitted Successfully!");
                ResetForm();
                LoadPendingClaims();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void UploadDocument(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf|Word Documents|*.docx|Excel Files|*.xlsx",
                Title = "Upload Supporting Document"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("File size exceeds the 5MB limit.");
                    return;
                }

                UploadedFileName.Text = fileInfo.FullName;
                MessageBox.Show("Document Uploaded Successfully!");
            }
        }

        private void ApproveClaim(object sender, RoutedEventArgs e)
        {
            UpdateClaimStatus("Approved");
        }

        private void RejectClaim(object sender, RoutedEventArgs e)
        {
            UpdateClaimStatus("Rejected");
        }

        private void UpdateClaimStatus(string status)
        {
            if (PendingClaims.SelectedItem is Claim selectedClaim)
            {
                using (var context = new AppDbContext())
                {
                    var claim = context.Claims.Find(selectedClaim.ClaimID);
                    if (claim != null)
                    {
                        claim.Status = status;
                        context.SaveChanges();
                    }
                }

                selectedClaim.Status = status;
                MessageBox.Show($"Claim {status}!");
                LoadPendingClaims();
            }
        }

        private void LoadPendingClaims()
        {
            using (var context = new AppDbContext())
            {
                PendingClaims.ItemsSource = context.Claims.Where(c => c.Status == "Pending").ToList();
            }
        }

        private void ResetForm()
        {
            LecturerName.Clear();
            HoursWorked.Clear();
            HourlyRate.Clear();
            AdditionalNotes.Clear();
            UploadedFileName.Text = "";
            ClaimStatus.Text = "Pending";
        }


        private void OnClaimSelected(object sender, RoutedEventArgs e)
        {
            if (PendingClaims.SelectedItem is Claim selectedClaim)
            {
                LecturerNameDetails.Text = selectedClaim.LecturerName;
                HoursWorkedDetails.Text = selectedClaim.HoursWorked.ToString();
                HourlyRateDetails.Text = selectedClaim.HourlyRate.ToString();
                TotalAmountDetails.Text = selectedClaim.TotalAmount.ToString("C");
                AdditionalNotesDetails.Text = selectedClaim.Notes;
                DocumentPathDetails.Text = selectedClaim.DocumentPath;

                // Set DataContext to update ClaimStatus automatically
                ClaimStatus.DataContext = selectedClaim;
            }
        }


        private void OpenHRReport(object sender, RoutedEventArgs e)
        {
            HRReport report = new HRReport();
            report.ShowDialog(); // Show the report window as a dialog
        }
    }

    public class Claim : INotifyPropertyChanged
    {
        private string _status;

        public int ClaimID { get; set; }
        public string LecturerName { get; set; }
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public double TotalAmount { get; set; }
        public string Notes { get; set; }
        public string DocumentPath { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


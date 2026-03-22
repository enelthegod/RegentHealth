using RegentHealth.Models;
using RegentHealth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RegentHealth.Views.Admin
{
    public partial class RotationPage : Page
    {
        private static readonly DayOfWeek[] WeekDays =
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        private int _weekOffset = 0;
        private List<Doctor> _doctors;

        private CheckBox[,] _workingChecks;
        private CheckBox[,] _emergencyChecks;

        public RotationPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => BuildWeek();
        }

        private void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            _weekOffset--;
            BuildWeek();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _weekOffset++;
            BuildWeek();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_doctors == null || _doctors.Count == 0)
            {
                MessageBox.Show("No doctors to save.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 1. Clear old rotations
            DataService.Instance.WeeklyRotations.Clear();

            // 2. Reset all doctor shift flags
            foreach (var doc in DataService.Instance.Doctors)
            {
                doc.IsOnShift = false;
                doc.IsEmergencyDoctor = false;
                doc.WorkingDays.Clear();
            }

            // 3. Save new rotation + update doctor flags
            for (int d = 0; d < 7; d++)
            {
                for (int i = 0; i < _doctors.Count; i++)
                {
                    bool working = _workingChecks[d, i].IsChecked == true;
                    bool emergency = _emergencyChecks[d, i].IsChecked == true;

                    if (working || emergency)
                    {
                        DataService.Instance.WeeklyRotations.Add(new DoctorRotation
                        {
                            Day = WeekDays[d],
                            DoctorId = _doctors[i].UserId,
                            IsEmergency = emergency
                        });

                        // Update Doctor object so scheduler can find them
                        var doc = _doctors[i];
                        doc.IsOnShift = true;

                        if (!doc.WorkingDays.Contains(WeekDays[d]))
                            doc.WorkingDays.Add(WeekDays[d]);

                        if (emergency)
                            doc.IsEmergencyDoctor = true;
                    }
                }
            }

            // Save rotations and updated doctor flags to DB
            DataService.Instance.SaveRotations();
            DataService.Instance.SaveDoctors();

            MessageBox.Show("Rotation saved! Doctors are now visible to patients.",
                "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }




        // UI logic
        private void BuildWeek()
        {
            _doctors = DataService.Instance.Doctors;
            int docCount = _doctors.Count;

            DateTime monday = GetMonday(DateTime.Today.AddDays(_weekOffset * 7));
            DateTime sunday = monday.AddDays(6);

            WeekRangeText.Text = $"{monday:dd MMM} – {sunday:dd MMM yyyy}";

            UpdateDayHeaders(monday);

            DoctorRowsPanel.Children.Clear();

            if (docCount == 0)
            {
                DoctorRowsPanel.Children.Add(new TextBlock
                {
                    Text = "No doctors found. Create doctors first.",
                    Style = (Style)FindResource("NoDoctorsText")
                });
                return;
            }

            _workingChecks = new CheckBox[7, docCount];
            _emergencyChecks = new CheckBox[7, docCount];

            var saved = DataService.Instance.WeeklyRotations;

            for (int i = 0; i < docCount; i++)
            {
                DoctorRowsPanel.Children.Add(
                    CreateDoctorRow(_doctors[i], i, saved));
            }
        }

        private void UpdateDayHeaders(DateTime monday)
        {
            for (int d = 0; d < 7; d++)
            {
                DateTime date = monday.AddDays(d);
                bool isToday = date.Date == DateTime.Today;

                if (FindName($"DayName{d}") is TextBlock nameBlock)
                    nameBlock.Text = date.ToString("ddd").ToUpper();

                if (FindName($"DayDate{d}") is TextBlock dateBlock)
                    dateBlock.Text = date.ToString("dd MMM");

                if (FindName($"DayHeader{d}") is Border headerBorder)
                    headerBorder.Style = (Style)FindResource(
                        isToday ? "DayHeaderToday" : "DayHeaderNormal");
            }
        }




        // Change ui directly here cause we dont know how many doctors will be by lines // later -> itemsControl + ViewModel
        private UIElement CreateDoctorRow(Doctor doc, int rowIdx,
                                          List<DoctorRotation> saved)
        {
            var grid = new Grid();
            grid.Style = (Style)FindResource("DoctorRowGrid");

            grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = (GridLength)FindResource("NameColumnWidth") });
            for (int d = 0; d < 7; d++)
                grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = (GridLength)FindResource("DayColumnWidth") });

            var nameCell = new TextBlock { Text = doc.FullName };
            nameCell.Style = (Style)FindResource("DoctorNameCell");
            Grid.SetColumn(nameCell, 0);
            grid.Children.Add(nameCell);

            for (int d = 0; d < 7; d++)
            {
                DayOfWeek day = WeekDays[d];
                var existing = saved.FirstOrDefault(
                    r => r.Day == day && r.DoctorId == doc.UserId);

                var chkWork = new CheckBox
                {
                    IsChecked = existing != null,
                    Style = (Style)FindResource("WorkingCheckBox")
                };
                var chkEmg = new CheckBox
                {
                    IsChecked = existing?.IsEmergency == true,
                    Style = (Style)FindResource("EmergencyCheckBox")
                };

                chkEmg.Checked += (s, ev) => chkWork.IsChecked = true;

                var cell = new StackPanel();
                cell.Style = (Style)FindResource("DayCell");
                cell.Children.Add(chkWork);
                cell.Children.Add(chkEmg);

                Grid.SetColumn(cell, d + 1);
                grid.Children.Add(cell);

                _workingChecks[d, rowIdx] = chkWork;
                _emergencyChecks[d, rowIdx] = chkEmg;
            }

            return grid;
        }



        private static DateTime GetMonday(DateTime date)
        {
            int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}
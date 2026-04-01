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
        private DateTime _currentMonday; // tracks which week is shown

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

            // Remove only rotations for THIS specific week (by date range)
            // Other weeks stay untouched
            var weekDates = Enumerable.Range(0, 7)
                .Select(i => _currentMonday.AddDays(i).Date)
                .ToList();

            DataService.Instance.WeeklyRotations
                .RemoveAll(r => weekDates.Contains(r.Date.Date));

            // Reset shift flags — will be rebuilt from all saved rotations
            foreach (var doc in DataService.Instance.Doctors)
            {
                doc.IsOnShift = false;
                doc.IsEmergencyDoctor = false;
                doc.WorkingDays = new List<DayOfWeek>();
            }

            // Save checked doctors for each day of THIS week
            for (int d = 0; d < 7; d++)
            {
                // Concrete date for this column
                DateTime date = _currentMonday.AddDays(d);

                for (int i = 0; i < _doctors.Count; i++)
                {
                    bool working = _workingChecks[d, i].IsChecked == true;
                    bool emergency = _emergencyChecks[d, i].IsChecked == true;

                    if (working || emergency)
                    {
                        DataService.Instance.WeeklyRotations.Add(new DoctorRotation
                        {
                            DoctorId = _doctors[i].Id,
                            Day = WeekDays[d],
                            Date = date,        // exact date, not just weekday
                            IsEmergency = emergency
                        });
                    }
                }
            }

            // Rebuild shift flags from ALL saved rotations (all weeks)
            // so switching between weeks doesn't break today's shifts
            var today = DateTime.Today;
            foreach (var rotation in DataService.Instance.WeeklyRotations)
            {
                var doc = DataService.Instance.Doctors
                    .FirstOrDefault(d => d.Id == rotation.DoctorId);

                if (doc == null) continue;

                doc.IsOnShift = true;

                if (!doc.WorkingDays.Contains(rotation.Day))
                    doc.WorkingDays.Add(rotation.Day);

                if (rotation.IsEmergency)
                    doc.IsEmergencyDoctor = true;
            }

            DataService.Instance.SaveRotations();
            DataService.Instance.SaveDoctors();

            MessageBox.Show("Rotation saved!",
                "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }


        // UI part TEMP
        private void BuildWeek()
        {
            _doctors = DataService.Instance.Doctors;
            int docCount = _doctors.Count;

            _currentMonday = GetMonday(DateTime.Today.AddDays(_weekOffset * 7));
            DateTime sunday = _currentMonday.AddDays(6);

            WeekRangeText.Text = $"{_currentMonday:dd MMM} – {sunday:dd MMM yyyy}";

            UpdateDayHeaders(_currentMonday);

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

            // Filter rotations only for THIS week's dates
            var weekDates = Enumerable.Range(0, 7)
                .Select(i => _currentMonday.AddDays(i).Date)
                .ToList();

            var thisWeekRotations = DataService.Instance.WeeklyRotations
                .Where(r => weekDates.Contains(r.Date.Date))
                .ToList();

            for (int i = 0; i < docCount; i++)
            {
                DoctorRowsPanel.Children.Add(
                    CreateDoctorRow(_doctors[i], i, thisWeekRotations));
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

        private UIElement CreateDoctorRow(Doctor doc, int rowIdx,
                                          List<DoctorRotation> thisWeekRotations)
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
                DateTime date = _currentMonday.AddDays(d);

                // Match by exact date AND doctor — not just weekday
                var existing = thisWeekRotations.FirstOrDefault(
                    r => r.Date.Date == date.Date && r.DoctorId == doc.Id);

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
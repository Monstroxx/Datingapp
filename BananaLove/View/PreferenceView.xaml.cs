using BananaLove.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BananaLove.View
{
    /// <summary>
    /// Interaktionslogik für PreferenceView.xaml
    /// </summary>
    public partial class PreferenceView : Window
    {
        Login login;
        public PreferenceView(Login login_data)
        {
            login = login_data;
            InitializeComponent();
        }

        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GenderButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton clickedButton = sender as ToggleButton;
            if (clickedButton == null || clickedButton.IsChecked != true) return;

            // Uncheck other buttons in the same group
            if (clickedButton == btnGenderFemale)
            {
                btnGenderMale.IsChecked = false;
                btnGenderOther.IsChecked = false;
            }
            else if (clickedButton == btnGenderMale)
            {
                btnGenderFemale.IsChecked = false;
                btnGenderOther.IsChecked = false;
            }
            else if (clickedButton == btnGenderOther)
            {
                btnGenderFemale.IsChecked = false;
                btnGenderMale.IsChecked = false;
            }
        }

        private void PreferenceGenderButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton clickedButton = sender as ToggleButton;
            if (clickedButton == null || clickedButton.IsChecked != true) return;

            // Uncheck other buttons in the same group
            if (clickedButton == btnPreferenceFemale)
            {
                btnPreferenceMale.IsChecked = false;
                btnPreferenceOther.IsChecked = false;
            }
            else if (clickedButton == btnPreferenceMale)
            {
                btnPreferenceFemale.IsChecked = false;
                btnPreferenceOther.IsChecked = false;
            }
            else if (clickedButton == btnPreferenceOther)
            {
                btnPreferenceFemale.IsChecked = false;
                btnPreferenceMale.IsChecked = false;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement save functionality
            // Get values:
            // - txtStreet.Text
            // - txtCity.Text
            // - Selected gender from btnGenderFemale/Male/Other
            // - dpBirthday.SelectedDate
            // - Selected preference gender from btnPreferenceFemale/Male/Other
            // - sliderRadius.Value
            string selectedGender = "";
            if (btnGenderFemale.IsChecked == true) { selectedGender = "w"; }
            else if (btnGenderMale.IsChecked == true) { selectedGender = "m"; }
            else if (btnGenderOther.IsChecked == true) { selectedGender = "d"; }

            string selectedPreferenceGender = "";
            if (btnPreferenceFemale.IsChecked == true) { selectedPreferenceGender = "w"; }
            else if (btnPreferenceMale.IsChecked == true) { selectedPreferenceGender = "m"; }
            else if (btnPreferenceOther.IsChecked == true) { selectedPreferenceGender = "d"; }
            
            int postalCode = 0;
            try { postalCode = int.Parse(txtPostalCode.Text); }
            catch (Exception)
            {
                MessageBox.Show("Bitte geben Sie eine gültige Postleitzahl ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime birthday;

            if (dpBirthday.SelectedDate != null)
            {
                birthday = (DateTime)dpBirthday.SelectedDate;
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie ein gültiges Geburtsdatum aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DBHandler.UpdateUserData(
                    login.UserID,
                    txtStreet.Text,
                    txtHouseNumber.Text,
                    txtCity.Text,
                    postalCode,
                    selectedGender,
                    birthday,
                    selectedPreferenceGender,
                    (int)sliderRadius.Value,
                    txtFirstName.Text,
                    txtLastName.Text,
                    txtBio.Text
                ))
            {
                MessageBox.Show("Einstellungen gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Es gab einen Fehler in der Speicherung", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}


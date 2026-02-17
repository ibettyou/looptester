using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Diagnostics;

namespace Loopback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LoopUtil _loop;
        private bool isDirty=false;
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _loop = new LoopUtil();
                _currentCulture = CultureInfo.CurrentUICulture;
                LoadResources();
                dgLoopback.ItemsSource = _loop.Apps;
                ICollectionView cvApps = CollectionViewSource.GetDefaultView(dgLoopback.ItemsSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error initializing application: {0}\n\nStack Trace:\n{1}", ex.Message, ex.StackTrace), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadResources()
        {
            try
            {
                if (_currentCulture != null && _currentCulture.Name == "zh-CN")
                {
                    _resourceManager = new ResourceManager("Loopback.Strings_zh_CN", Assembly.GetExecutingAssembly());
                }
                else
                {
                    _resourceManager = new ResourceManager("Loopback.Strings_en", Assembly.GetExecutingAssembly());
                }
                ApplyResources();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading resources: {0}", ex.Message), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyResources()
        {
            try
            {
                var resources = new Dictionary<string, string>();
                resources["WindowTitle"] = _resourceManager.GetString("WindowTitle") ?? "Loopback Exemption Manager";
                resources["SaveButton"] = _resourceManager.GetString("SaveButton") ?? "Save";
                resources["RefreshButton"] = _resourceManager.GetString("RefreshButton") ?? "Refresh";
                resources["ExemptColumn"] = _resourceManager.GetString("ExemptColumn") ?? "Exempt";
                resources["AppNameColumn"] = _resourceManager.GetString("AppNameColumn") ?? "App Name";
                resources["StatusLabel"] = _resourceManager.GetString("StatusLabel") ?? "Status:";
                resources["LanguageButton"] = _resourceManager.GetString("LanguageButton") ?? "中文";
                resources["SelectAllButton"] = _resourceManager.GetString("SelectAllButton") ?? "Select All";
                resources["DeselectAllButton"] = _resourceManager.GetString("DeselectAllButton") ?? "Deselect All";

                foreach (var key in resources.Keys)
                {
                    this.Resources[key] = resources[key];
                }

                this.Title = resources["WindowTitle"];
                btnLanguage.Content = resources["LanguageButton"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error applying resources: {0}", ex.Message), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentCulture.Name == "zh-CN")
            {
                _currentCulture = new CultureInfo("en");
            }
            else
            {
                _currentCulture = new CultureInfo("zh-CN");
            }
            LoadResources();
        }

        private void btnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = "https://github.com/appshubcc/Windows-Loopback-Manager";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error opening URL: {0}", ex.Message), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = true;
            }
            dgLoopback.Items.Refresh();
            isDirty = true;
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _loop.Apps)
            {
                app.LoopUtil = false;
            }
            dgLoopback.Items.Refresh();
            isDirty = true;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isDirty) 
            {
                Log(_resourceManager.GetString("NothingToSave"));
                return; 
            }

            isDirty = false;
            if (_loop.SaveLoopbackState())
            { 
                Log(_resourceManager.GetString("SavedExemptions"));
            }
            else
            { Log(_resourceManager.GetString("ErrorSaving")); }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _loop.LoadApps();
            dgLoopback.Items.Refresh();
            isDirty = false;
            Log(_resourceManager.GetString("Refreshed"));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isDirty)
            {
                string title = _resourceManager.GetString("UnsavedChangesTitle");
                string message = _resourceManager.GetString("UnsavedChangesMessage");
                MessageBoxResult resp=System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resp==MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

            }
            _loop.FreeResources();
        }

        private void dgcbLoop_Click(object sender, RoutedEventArgs e)
        {
            isDirty=true;
        }

        private void Log(String logtxt) 
        {
                txtStatus.Text = DateTime.Now.ToString("HH:mm:ss.fff ") + logtxt;
        }

    }
}

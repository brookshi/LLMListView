using ListViewSample.Model;
using LLM;
using LLMListView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Demo.Pages
{
    public sealed partial class NormalFixPageExtended : Page
    {
        ObservableCollection<Contact> _contacts = new ObservableCollection<Contact>();

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set { _contacts = value; }
        }

        Dictionary<Contact, bool> ContackStarDict = new Dictionary<Contact, bool>();

        public string Culture { get; set; } = "US";

        public NormalFixPageExtended()
        {
            this.InitializeComponent();
            DataContext = this;
            AddSwipeTemplate();
            Contacts = Contact.GetContacts(140);

            EditCommand = new DelayCommand<Contact>(async itemData =>
            {
                var dlg = new MessageDialog("Edit " + itemData.Name);
                dlg.Commands.Add(new UICommand("OK"));
                dlg.Commands.Add(new UICommand("Cancel"));
                await dlg.ShowAsync();
            });
            DelCommand = new DelayCommand<Contact>(async itemData =>
            {
                var dlg = new MessageDialog("Comfire Delete " + itemData.Name + "?");
                dlg.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(param =>
                {
                    _contacts.Remove(itemData);
                })));
                dlg.Commands.Add(new UICommand("Cancel"));
                await dlg.ShowAsync();
            });
        }
        

        private void AddSwipeTemplate()
        {
            MasterListView.ItemRightSwipeContentTemplate = CreateTemplate();
        }

        DataTemplate CreateTemplate()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"> ");
            stringBuilder.Append("<Grid Background=\"Red\">");
            stringBuilder.Append("<StackPanel HorizontalAlignment=\"Right\" Orientation=\"Horizontal\">");
            stringBuilder.Append("<AppBarButton Name=\"Edit\"  Background=\"Green\" Command=\"{Binding DataContext.EditCommand, ElementName=MasterListView}\" CommandParameter=\"{ Binding }\" Click=\"Edit_Click\" Icon=\"Edit\" Label=\"Edit\" />");
            stringBuilder.Append("<AppBarButton Name=\"Delete\" Background=\"Red\" Command=\"{Binding DataContext.DelCommand, ElementName=MasterListView}\" CommandParameter=\"{ Binding }\"  Click=\"Delete_Click\" Icon=\"Delete\" Label=\"Delete\" />");
            stringBuilder.Append("</StackPanel>");
            stringBuilder.Append("</Grid>");
            stringBuilder.Append("</DataTemplate>");
            var template = (DataTemplate)XamlReader.Load(stringBuilder.ToString()); 
            return template;
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            var item = Utils.FindVisualParent<LLMListViewItem>(sender as AppBarButton);
            var itemData = item.Content as Contact;
            var dlg = new MessageDialog("Edit " + itemData.Name);
            dlg.Commands.Add(new UICommand("OK"));
            dlg.Commands.Add(new UICommand("Cancel"));
            await dlg.ShowAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = Utils.FindVisualParent<LLMListViewItem>(sender as AppBarButton);
            var itemData = item.Content as Contact;
            var dlg = new MessageDialog("Comfire Delete " + itemData.Name + "?");
            dlg.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(param =>
            {
                _contacts.Remove(itemData);
            })));
            dlg.Commands.Add(new UICommand("Cancel"));
            await dlg.ShowAsync();
        }

        public DelayCommand<Contact> EditCommand { get; set; }

        public DelayCommand<Contact> DelCommand { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ContosoCheckIn.UserControls
{
    /// <summary>
    /// Interaction logic for MoreCandidatesUserControl.xaml
    /// </summary>
    public partial class MoreCandidatesUserControl : UserControl
    {
        ParticipantIdentifyResult[] identifyResults;
        EventHandler EventHandler;
        public MoreCandidatesUserControl(ParticipantIdentifyResult[] participant, EventHandler eventhandler)
        {
            identifyResults = participant;
            InitializeComponent();
            EventHandler = eventhandler;
            GenerateListBoxItems();
            SoundNotification.PlayWarningSound();
        }

        private List<string> IDs;

        private void GenerateListBoxItems()
        {
            IDs = new List<string>();
            results.Items.Clear();
            foreach (ParticipantIdentifyResult identifyResult in identifyResults)
            {
                IDs.Add(identifyResult.participant.id);

                Grid grid = new Grid();
                grid.Margin = new Thickness(0, 2, 0, 2);

                ColumnDefinition cd1 = new ColumnDefinition();
                cd1.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(cd1);

                ColumnDefinition cd2 = new ColumnDefinition();
                cd2.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(cd2);

                Grid grid2 = new Grid();

                RowDefinition rd1 = new RowDefinition();
                rd1.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd1);

                RowDefinition rd2 = new RowDefinition();
                rd2.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd2);

                RowDefinition rd3 = new RowDefinition();
                rd3.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd3);

                RowDefinition rd4 = new RowDefinition();
                rd4.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd4);

                RowDefinition rd5 = new RowDefinition();
                rd5.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd5);

                RowDefinition rd6 = new RowDefinition();
                rd6.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd6);

                /*RowDefinition rd7 = new RowDefinition();
                rd7.Height = GridLength.Auto;
                grid2.RowDefinitions.Add(rd7);*/

                Grid.SetColumn(grid2, 1); grid.Children.Add(grid2);

                Image image = new Image();
                image.Height = 130;
                image.Width = 130;
                image.Source = identifyResult.participant.faces[0].Source();
                Grid.SetColumn(image, 0); grid.Children.Add(image);

                TextBlock tb = new TextBlock();
                tb.FontSize = 25;
                tb.HorizontalAlignment = HorizontalAlignment.Left;
                Thickness margin = new Thickness(3, 1, 3, 0);
                tb.Margin = margin;
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.Text = identifyResult.participant.firstName + " " + identifyResult.participant.lastName;
                Grid.SetRow(tb, 0); grid2.Children.Add(tb);

                TextBlock tb2 = new TextBlock();
                tb2.FontSize = 14;
                tb2.HorizontalAlignment = HorizontalAlignment.Left;
                tb2.Margin = margin;
                tb2.Foreground = new SolidColorBrush(Colors.White);
                tb2.Text = identifyResult.participant.birth;
                Grid.SetRow(tb2, 1); grid2.Children.Add(tb2);

                TextBlock tb3 = new TextBlock();
                tb3.FontSize = 14;
                tb3.HorizontalAlignment = HorizontalAlignment.Left;
                tb3.Margin = margin;
                tb3.Foreground = new SolidColorBrush(Colors.White);
                tb3.Text = identifyResult.participant.email;
                Grid.SetRow(tb3, 2); grid2.Children.Add(tb3);

                TextBlock tb4 = new TextBlock();
                tb4.FontSize = 14;
                tb4.HorizontalAlignment = HorizontalAlignment.Left;
                tb4.Margin = margin;
                tb4.Foreground = new SolidColorBrush(Colors.White);
                tb4.Text = identifyResult.participant.company;
                Grid.SetRow(tb4, 3); grid2.Children.Add(tb4);

                TextBlock tb5 = new TextBlock();
                tb5.FontSize = 14;
                tb5.HorizontalAlignment = HorizontalAlignment.Left;
                tb5.Margin = margin;
                tb5.Foreground = new SolidColorBrush(Colors.White);
                tb5.Text = identifyResult.participant.workTitle;
                Grid.SetRow(tb5, 4); grid2.Children.Add(tb5);

                TextBlock tb6 = new TextBlock();
                tb6.FontSize = 18;
                tb6.HorizontalAlignment = HorizontalAlignment.Right;
                tb6.Margin = margin;
                tb6.Foreground = new SolidColorBrush(Colors.White);
                tb6.Text = (int)(identifyResult.confidence * 100) + "%";
                Grid.SetRow(tb6, 5); grid2.Children.Add(tb6);

                /* TextBlock tb7 = new TextBlock();
                tb7.FontSize = 15;
                tb7.HorizontalAlignment = HorizontalAlignment.Right;
                tb7.Margin = margin;
                tb7.Foreground = new SolidColorBrush(Colors.White);
                tb7.Text = identifyResult.participant.groupName;
                Grid.SetRow(tb7, 6); grid2.Children.Add(tb7);*/

                results.Items.Add(grid);
            }
        }

        private void results_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EventHandler.resetBlock();

            // Log action to the API
            ApiClient.LogSelectedParticipant(identifyResults[results.SelectedIndex]);

            // Create a single login event to show the result
            NewIdentifyEventArgs args = new NewIdentifyEventArgs();
            args.Faces = new ParticipantIdentifyResult[] { identifyResults[results.SelectedIndex] };
            EventHandler.OnIdentifyResultProvided(args);
        }
    }
}
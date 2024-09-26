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

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for GameOptions.xaml
    /// </summary>
    public partial class GameOptions : Window
    {
        private string chosenBoardDesign = "White/Black";
        private string chosenSnakeDesign = "Green";
        private List<string> boardDesigns;
        private List<string> snakeDesigns;
        private string _checkedButton = "";

        private enum _gameDifficulty
        { Easy, Normal, Hard, Nightmare }

        public GameOptions()
        {
            InitializeComponent();
            boardDesigns = new List<string>();
            snakeDesigns = new List<string>();
            InsertIntoDropdowns();
        }

        private void DifficultyChecked(object sender, RoutedEventArgs e)
        {
            if (BtnEasy.IsChecked == true)
            {
                _checkedButton = "Easy";
            }
            else if (BtnNormal.IsChecked == true)
            {
                _checkedButton = "Normal";
            }
            else if (BtnHard.IsChecked == true)
            {
                _checkedButton = "Hard";
            }
            else if (BtnNightmare.IsChecked == true)
            {
                _checkedButton = "Nightmare";
            }
            else
            {
                MessageBox.Show("Something went wrong. Please try again...");
            }
        }

        private void BtnStartGame_Click(object sender, RoutedEventArgs e)
        {
            if (_checkedButton != "" && _checkedButton is not null)
            {
                if (cbbBoardDesign.SelectedItem is not null && cbbSnakeDesign.SelectedItem is not null)
                {
                    chosenBoardDesign = cbbBoardDesign.SelectedItem.ToString();
                    chosenSnakeDesign = cbbSnakeDesign.SelectedItem.ToString();
                }
                MainWindow window = new MainWindow(_checkedButton, chosenBoardDesign, chosenSnakeDesign);
                window.Show();
                this.Close();
            }
        }

        private void BtnHighscore_Click(object sender, RoutedEventArgs e)
        {
        }

        private void InsertIntoDropdowns()
        {
            //Hard coded designs for now, will/should be changed later
            boardDesigns.Add("White/Black");
            boardDesigns.Add("Green");
            boardDesigns.Add("Red/White");
            boardDesigns.Add("Blue");
            boardDesigns.Add("Yellow");
            boardDesigns.Add("Gray");
            boardDesigns.Add("Red");
            snakeDesigns.Add("Green");
            snakeDesigns.Add("Red");
            snakeDesigns.Add("Blue");
            snakeDesigns.Add("Yellow");
            snakeDesigns.Add("Black");
            snakeDesigns.Add("White");
            snakeDesigns.Add("Brown");

            foreach (var design in boardDesigns)
            {
                if (!cbbBoardDesign.Items.Contains(design))
                {
                    cbbBoardDesign.Items.Add(design);
                }
            }
            foreach (var design in snakeDesigns)
            {
                if (!cbbSnakeDesign.Items.Contains(design))
                {
                    cbbSnakeDesign.Items.Add(design);
                }
            }
        }
    }
}
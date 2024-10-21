using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SnakeGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private int actionsTaken = 0;
        private bool gameEnded = false;
        private int SnakeSquareSize = 50;
        private const int SnakeStartLength = 3;
        private int SnakeStartSpeed = 400;
        private int SnakeSpeedThreshold = 100;
        private const int MaxHighscoreListEntryCount = 5;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();

        private string fruitsPath = @"C:\\Users\U427797\source\repos\SnakeGame\SnakeGame\Fruits\";
        private List<string> Food = new();

        public enum SnakeDirection
        { Left, Right, Up, Down }

        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private Random rnd = new Random();
        private UIElement snakeFood = null;
        private int currentscore = 0;

        private string _difficulty;
        private string _boardDesign;
        private string _snakeDesign;
        private string _foodDesign;
        private bool NightMareMode = false;

        public ObservableCollection<SnakeHighscore> HighscoreList
        {
            get; set;
        } = new ObservableCollection<SnakeHighscore>();

        public MainWindow(string ChosenDifficulty, string ChosenBoardDesign, string ChosenSnakeDesign, string ChosenFoodDesign)
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            _boardDesign = ChosenBoardDesign;
            _snakeDesign = ChosenSnakeDesign;
            _foodDesign = ChosenFoodDesign;
            _difficulty = ChosenDifficulty;
            LoadHighscoreList();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DifficultyManagement();
            DesignBoard();
            DesignSnake();
            DesignFood();
        }

        private void StartNewGame()
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;
            bdrNewHighscore.Visibility = Visibility.Collapsed;

            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UIElement != null)
                {
                    GameArea.Children.Remove(snakeBodyPart.UIElement);
                }
            }
            snakeParts.Clear();
            if (snakeFood != null)
            {
                GameArea.Children.Remove(snakeFood);
            }

            //Reset a few things
            gameEnded = false;
            currentscore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            //Draw the snake and some food
            DrawSnake();
            DrawSnakeFood();

            //Update status
            UpdateGameStatus();

            //start the game
            gameTickTimer.IsEnabled = true;
        }

        private void DrawGameArea(Brush color1, Brush color2)
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? color1 : color2
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                {
                    doneDrawingBackground = true;
                }
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UIElement == null)
                {
                    snakePart.UIElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UIElement);
                    Canvas.SetTop(snakePart.UIElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UIElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            //Removes the last part of the snake to prepare for the new part that gets added below
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UIElement);
                snakeParts.RemoveAt(0);
            }

            //Now we add a new element to the snake which will be the new snake head and we will move everything that isn't a head to that body instead
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UIElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            //Based on the current direction then determine which direction the snake should expand
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;

                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;

                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;

                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            //Now we add the new head part to our list of snake parts
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            //Draw snake again part
            DrawSnake();

            //Collision check time!!!
            DoCollisionCheck();
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY)) { return GetNextFoodPosition(); }
            }

            return new Point(foodX, foodY);
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            string nextFruit = Food[rnd.Next(Food.Count)];
            Image i = new Image();
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(nextFruit, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            i.Source = src;
            i.MaxHeight = SnakeSquareSize;
            i.MaxWidth = SnakeSquareSize;
            snakeFood = i;
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void Window_Keyup(object sender, KeyEventArgs e)
        {
            if (gameEnded)
            {
                if (e.Key == Key.Space)
                {
                    StartNewGame();
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.W:
                    case Key.Up:
                        if (snakeDirection != SnakeDirection.Down && !NightMareMode && actionsTaken < 1)
                            snakeDirection = SnakeDirection.Up;
                        else if (snakeDirection != SnakeDirection.Right && actionsTaken < 1)
                        {
                            snakeDirection = SnakeDirection.Down;
                        }
                        actionsTaken++;
                        break;

                    case Key.S:
                    case Key.Down:
                        if (snakeDirection != SnakeDirection.Up && !NightMareMode && actionsTaken < 1)
                            snakeDirection = SnakeDirection.Down;
                        else if (snakeDirection != SnakeDirection.Right && actionsTaken < 1)
                        {
                            snakeDirection = SnakeDirection.Up;
                        }
                        actionsTaken++;
                        break;

                    case Key.Left:
                    case Key.A:
                        if (snakeDirection != SnakeDirection.Right && !NightMareMode && actionsTaken < 1)
                            snakeDirection = SnakeDirection.Left;
                        else if (snakeDirection != SnakeDirection.Right && actionsTaken < 1)
                        {
                            snakeDirection = SnakeDirection.Right;
                        }
                        actionsTaken++;
                        break;

                    case Key.Right:
                    case Key.D:
                        if (snakeDirection != SnakeDirection.Left && !NightMareMode && actionsTaken < 1)
                            snakeDirection = SnakeDirection.Right;
                        else if (snakeDirection != SnakeDirection.Right && actionsTaken < 1)
                        {
                            snakeDirection = SnakeDirection.Left;
                        }
                        actionsTaken++;
                        break;

                    case Key.F2:
                        GameOptions options = new GameOptions();
                        options.Show();
                        this.Close();
                        break;

                    case Key.Space:
                        StartNewGame();
                        break;

                    case Key.Escape:
                        this.Close();
                        break;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            this.DragMove();
        }

        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) || (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                {
                    EndGame();
                }
            }
        }

        private void EatSnakeFood()
        {
            snakeLength++;
            currentscore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentscore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        private void DesignBoard()
        {
            switch (_boardDesign)
            {
                case "White/Black":
                    DrawGameArea(Brushes.White, Brushes.Black);
                    break;

                case "Red/White":
                    DrawGameArea(Brushes.White, Brushes.Red);
                    break;

                case "Gray":
                    DrawGameArea(Brushes.SlateGray, Brushes.Gray);
                    break;

                case "Blue":
                    DrawGameArea(Brushes.RoyalBlue, Brushes.Blue);
                    break;

                case "Yellow":
                    DrawGameArea(Brushes.LightGoldenrodYellow, Brushes.LightYellow);
                    break;

                case "Red":
                    DrawGameArea(Brushes.OrangeRed, Brushes.Red);
                    break;

                case "Green":
                    DrawGameArea(Brushes.LimeGreen, Brushes.SpringGreen);
                    break;
            }
        }

        private void DesignSnake()
        {
            switch (_snakeDesign)
            {
                case "Brown":
                    snakeBodyBrush = Brushes.Brown;
                    snakeHeadBrush = Brushes.SandyBrown;
                    break;

                case "Blue":
                    snakeBodyBrush = Brushes.DarkBlue;
                    snakeHeadBrush = Brushes.DodgerBlue;
                    break;

                case "Yellow":
                    snakeBodyBrush = Brushes.Yellow;
                    snakeHeadBrush = Brushes.YellowGreen;
                    break;

                case "Red":
                    snakeBodyBrush = Brushes.DarkRed;
                    snakeHeadBrush = Brushes.IndianRed;
                    break;

                case "Green":
                    snakeBodyBrush = Brushes.DarkGreen;
                    snakeHeadBrush = Brushes.PaleGreen;
                    break;

                case "Black":
                    snakeBodyBrush = Brushes.Black;
                    snakeHeadBrush = Brushes.DarkGray;
                    break;

                case "White":
                    snakeBodyBrush = Brushes.FloralWhite;
                    snakeHeadBrush = Brushes.LightGray;
                    break;
            }
        }

        private void DesignFood()
        {
            Food.Clear();
            switch (_foodDesign)
            {
                case "Fruit":
                    Food.Add(fruitsPath + "apple.png");
                    Food.Add(fruitsPath + "banana.png");
                    Food.Add(fruitsPath + "cherry.png");
                    Food.Add(fruitsPath + "grape.png");
                    Food.Add(fruitsPath + "kiwi.png");
                    Food.Add(fruitsPath + "orange.png");
                    Food.Add(fruitsPath + "peach.png");
                    Food.Add(fruitsPath + "pineapple.png");
                    Food.Add(fruitsPath + "raspberry.png");
                    Food.Add(fruitsPath + "strawberry.png");
                    Food.Add(fruitsPath + "watermelon.png");
                    Food.Add(fruitsPath + "peach.png");
                    Food.Add(fruitsPath + "blueberry.png");
                    break;

                case "Vegetables":
                    Food.Add(fruitsPath + "carrots.png");
                    Food.Add(fruitsPath + "eggplant.png");
                    Food.Add(fruitsPath + "jalapeno.png");
                    Food.Add(fruitsPath + "lemon.png");
                    Food.Add(fruitsPath + "pumpkin.png");
                    Food.Add(fruitsPath + "mushroom.png");
                    Food.Add(fruitsPath + "raddish.png");
                    Food.Add(fruitsPath + "tomato.png");
                    break;

                case "Only Green":
                    Food.Add(fruitsPath + "watermelon.png");
                    Food.Add(fruitsPath + "kiwi.png");
                    Food.Add(fruitsPath + "jalapeno.png");
                    break;

                case "Only Red":
                    Food.Add(fruitsPath + "apple.png");
                    Food.Add(fruitsPath + "cherry.png");
                    Food.Add(fruitsPath + "mushroom.png");
                    Food.Add(fruitsPath + "raspberry.png");
                    Food.Add(fruitsPath + "strawberry.png");
                    Food.Add(fruitsPath + "watermelon.png");
                    Food.Add(fruitsPath + "tomato.png");
                    break;

                case "Only Purple":
                    Food.Add(fruitsPath + "eggplant.png");
                    Food.Add(fruitsPath + "grape.png");
                    Food.Add(fruitsPath + "lemon.png");
                    Food.Add(fruitsPath + "raddish.png");
                    break;

                case "Only Orange":
                    Food.Add(fruitsPath + "carrots.png");
                    Food.Add(fruitsPath + "orange.png");
                    Food.Add(fruitsPath + "pumpkin.png");
                    break;
            }
        }

        private void DifficultyManagement()
        {
            switch (_difficulty)
            {
                case "Easy":
                    GameArea.Height = 800;
                    GameArea.Width = 1300;
                    SnakeStartSpeed = 400;
                    break;

                case "Normal":
                    GameArea.Width = 1400;
                    GameArea.Height = 800;
                    SnakeSquareSize = 40;
                    SnakeStartSpeed = 350;
                    break;

                case "Hard":
                    GameArea.Width = 1450;
                    GameArea.Height = 900;
                    SnakeSquareSize = 30;
                    SnakeStartSpeed = 275;
                    SnakeSpeedThreshold = 50;
                    break;

                case "Nightmare":
                    GameArea.Height = 900;
                    GameArea.Width = 1400;
                    SnakeSquareSize = 20;
                    SnakeStartSpeed = 200;
                    SnakeSpeedThreshold = 15;
                    NightMareMode = true;
                    break;
            }
        }

        private void LoadHighscoreList()
        {
            if (File.Exists("snake_highscorelist.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open))
                {
                    List<SnakeHighscore> templist = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    this.HighscoreList.Clear();
                    foreach (var item in templist.OrderByDescending(x => x.Score))
                    {
                        this.HighscoreList.Add(item);
                    }
                }
            }
        }

        private void SaveHighscoreList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream("snake_highscorelist.xml", FileMode.Create))
            {
                serializer.Serialize(writer, this.HighscoreList);
            }
        }

        private void BtnClearHighscoreList_Click(object sender, EventArgs e)
        {
            this.HighscoreList.Clear();
        }

        private void BtnAddToHighscoreList_Click(object sender, EventArgs e)
        {
            int newIndex = 0;

            if ((this.HighscoreList.Count > 0) && (currentscore < this.HighscoreList.Max(x => x.Score)))
            {
                SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentscore);
                if (justAbove != null)
                {
                    newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
                }
            }

            //Create and insert a new entry
            this.HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = currentscore,
            });

            while (this.HighscoreList.Count > MaxHighscoreListEntryCount)
            {
                this.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);
            }

            SaveHighscoreList();

            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        private void BtnShowHighscoreList_Click(object sender, EventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        private void UpdateGameStatus()
        {
            this.tbStatusScore.Text = currentscore.ToString();
            this.tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
        }

        private void EndGame()
        {
            bool isNewHighscore = false;
            if (currentscore > 0)
            {
                int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
                if ((currentscore > lowestHighscore) || (this.HighscoreList.Count < MaxHighscoreListEntryCount))
                {
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }

            if (!isNewHighscore)
            {
                tbFinalScore.Text = currentscore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }

            gameTickTimer.IsEnabled = false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            actionsTaken = 0;
            MoveSnake();
        }
    }
}
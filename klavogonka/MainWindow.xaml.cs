using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Klavagonochka
{
    public partial class MainWindow : Window
    {
        private string textToType = "";
        private string typedText = "";
        private Random random = new Random();
        private int currentIndex = 0;
        private DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            int length = (int)DifficultySlider.Value;
            textToType = new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
            UpdateTextToTypeDisplay();
        }

        private void UpdateTextToTypeDisplay()
        {
            TextToType.Inlines.Clear();

            for (int i = 0; i < textToType.Length; i++)
            {
                var run = new Run(textToType[i].ToString())
                {
                    Background = i == currentIndex ? Brushes.Yellow : Brushes.Transparent,
                    FontWeight = i == currentIndex ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = i < currentIndex ? Brushes.Gray : Brushes.Black
                };

                TextToType.Inlines.Add(run);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            typedText = "";
            currentIndex = 0;
            InputTextBox.Clear();
            GenerateRandomText();
            ResultText.Text = "";
            startTime = DateTime.Now;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            CheckResult();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (typedText.Length > 0)
                {
                    typedText = typedText.Substring(0, typedText.Length - 1);
                    currentIndex = Math.Max(0, currentIndex - 1);
                }
            }
            else
            {
                char keyChar = GetCharFromKey(e.Key);

                if (currentIndex < textToType.Length && keyChar != '\0')
                {
                    if (keyChar == textToType[currentIndex])
                    {
                        typedText += keyChar;
                        currentIndex++;
                        HighlightKey(keyChar, Brushes.Green);
                    }
                    else
                    {
                        HighlightKey(keyChar, Brushes.Red);
                    }
                }
            }

            UpdateTextToTypeDisplay();
            CheckResult();
        }

        private char GetCharFromKey(Key key)
        {
            if (key >= Key.A && key <= Key.Z)
            {
                bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                char ch = (char)('a' + (key - Key.A));
                return isShiftPressed ? char.ToUpper(ch) : ch;
            }
            else if (key >= Key.D0 && key <= Key.D9)
            {
                return (char)('0' + (key - Key.D0));
            }
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return (char)('0' + (key - Key.NumPad0));
            }

            return key switch
            {
                Key.Space => ' ',
                Key.OemComma => ',',
                Key.OemPeriod => '.',
                Key.OemMinus => '-',
                Key.OemPlus => '+',
                _ => '\0'
            };
        }

        private void HighlightKey(char keyChar, Brush color)
        {
            string keyString = keyChar.ToString().ToUpper();

            foreach (var child in MainGrid.Children)
            {
                if (child is Button button && button.Tag != null && button.Tag.ToString() == keyString)
                {
                    var originalBackground = button.Background;
                    button.Background = color;
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                    timer.Tick += (s, e) =>
                    {
                        button.Background = originalBackground;
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }

        private void CheckResult()
        {
            if (currentIndex == textToType.Length)
            {
                TimeSpan timeTaken = DateTime.Now - startTime;
                double speed = textToType.Length / timeTaken.TotalMinutes;
                ResultText.Text = $"Успех! Ваша скорость: {speed:F2} символов в минуту";
                ResultText.Foreground = Brushes.Green;
            }
        }
    }
}

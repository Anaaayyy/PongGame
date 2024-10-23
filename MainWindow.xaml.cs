using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PongGame
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer gameTimer; // Таймер для управления игровым циклом
        private double ballSpeedX = 100, ballSpeedY = 70;// Скорости мяча по оси X и Y
        private int player1Score = 0, player2Score = 0; // Счет игроков
        private const int winningScore = 5; // Количество очков для победы

        private bool isPlayer1Active = true; // Ракетка игрока 1 активна
        private bool isPlayer2Active = true; // Ракетка игрока 2 активна


        public MainWindow()
        {
            InitializeComponent();
            // Инициализация таймера с интервалом 16 миллисекунд (приблизительно 60 кадров в секунду)
            gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            // Обработка события "Tick" таймера
            gameTimer.Tick += (s, e) => { MoveBall(); CheckCollisions(); CheckForWinner(); };
        }

        private void MoveBall()
        {
            // Перемещение мяча по текущим скоростям
            Canvas.SetLeft(Ball, Canvas.GetLeft(Ball) + ballSpeedX);
            Canvas.SetTop(Ball, Canvas.GetTop(Ball) + ballSpeedY);

            // Блокировка ракеток при пересечении мяча середины поля
            double ballCenterX = Canvas.GetLeft(Ball) + Ball.Width / 2;
            if (ballCenterX > GameCanvas.ActualWidth / 2)
            {
                isPlayer1Active = false; // Блокируем игрока 1
                isPlayer2Active = true;  // Разблокируем игрока 2
            }
            else if (ballCenterX < GameCanvas.ActualWidth / 2)
            {
                isPlayer1Active = true;  // Разблокируем игрока 1
                isPlayer2Active = false; // Блокируем игрока 2
            }
        }


        private void CheckCollisions()
        {
            // Проверка столкновений с верхней и нижней границей
            if (Canvas.GetTop(Ball) <= 0 || Canvas.GetTop(Ball) + Ball.Height >= GameCanvas.ActualHeight)
                ballSpeedY *= -1; // Изменение направления по Y

            // Проверка столкновений с игроками
            if (IsColliding(Player1))
                ReflectBall(Player1, ref ballSpeedX, Math.Abs(ballSpeedX)); // Отражение мяча от игрока 1
            else if (IsColliding(Player2))
                ReflectBall(Player2, ref ballSpeedX, -Math.Abs(ballSpeedX)); // Отражение мяча от игрока 2
            else
                CheckOutOfBounds(); // Проверка, не вышел ли мяч за границы поля
        }

        private void ReflectBall(Rectangle player, ref double ballSpeed, double newDirection)
        {
            ballSpeed = newDirection; // Установка новой скорости мяча
             // Вычисление новой скорости по Y в зависимости от места удара
            ballSpeedY = ((Canvas.GetTop(Ball) + Ball.Height / 2) - (Canvas.GetTop(player) + player.Height / 2)) / (player.Height / 2) * 10;
            // Установка позиции мяча в зависимости от игрока, от которого он отскочил
            Canvas.SetLeft(Ball, player == Player1 ? Canvas.GetLeft(player) + player.Width : Canvas.GetLeft(player) - Ball.Width);
        }

        private void CheckOutOfBounds()
        {
            // Проверка выхода мяча за границы слева
            if (Canvas.GetLeft(Ball) < 0)
            {
                if (!IsColliding(Player1)) // Если мяч не столкнулся с игроком 1, начисляем очко игроку 2
                    ScorePoint(ref player2Score, false);
            }
            // Проверка выхода мяча за границы справа
            else if (Canvas.GetLeft(Ball) + Ball.Width > GameCanvas.ActualWidth)
            {
                if (!IsColliding(Player2)) // Если мяч не столкнулся с игроком 2, начисляем очко игроку 1
                    ScorePoint(ref player1Score, true);
            }
        }

        private void ScorePoint(ref int playerScore, bool directionToPlayer1)
        {
            playerScore++; // Увеличиваем счет игрока
            UpdateScores(); // Обновляем отображение счета
            ResetBall(directionToPlayer1); // Сбрасываем мяч
        }

        private bool IsColliding(Rectangle player)
        {
            // Проверка на столкновение мяча с игроком
            return Canvas.GetLeft(Ball) < Canvas.GetLeft(player) + player.Width &&
                   Canvas.GetLeft(Ball) + Ball.Width > Canvas.GetLeft(player) &&
                   Canvas.GetTop(Ball) < Canvas.GetTop(player) + player.Height &&
                   Canvas.GetTop(Ball) + Ball.Height > Canvas.GetTop(player);
        }

        private void UpdateScores()
        {
            // Обновление текстовых меток счета игроков
            ScoreLabel1.Text = $"{Player1Name.Text}: {player1Score}";
            ScoreLabel2.Text = $"{Player2Name.Text}: {player2Score}";
        }

        private void CheckForWinner()
        {
            // Проверка условия победы
            if (player1Score == winningScore || player2Score == winningScore)
            {
                string winner = player1Score == winningScore ? Player1Name.Text : Player2Name.Text; // Определение победителя
                gameTimer.Stop(); // Остановка игры
                // Запрос на перезапуск игры
                if (MessageBox.Show($"{winner} выиграл! Сыграть снова?", "Конец игры", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    RestartGame(); // Перезапуск игры
                else
                    Application.Current.Shutdown(); // Закрытие приложения
            }
        }

        private void ResetBall(bool directionToPlayer1)
        {
            // Сброс мяча в центр игрового поля
            Canvas.SetLeft(Ball, (GameCanvas.ActualWidth - Ball.Width) / 2);
            Canvas.SetTop(Ball, (GameCanvas.ActualHeight - Ball.Height) / 2);
            // Установка скорости мяча в зависимости от направления
            ballSpeedX = directionToPlayer1 ? 4 : -4;
            ballSpeedY = new Random().Next(-3, 4) + 5; // Увеличиваем скорость по Y
            // Убедитесь, что скорость по Y не равна нулю
            if (ballSpeedY == 0)
                ballSpeedY = 2;
        }

        private void RestartGame()
        {
            // Сброс счетов и обновление интерфейса
            player1Score = player2Score = 0;
            UpdateScores();
            ResetBall(new Random().Next(0, 2) == 0);
            Player1Name.Text = "Игрок 1";
            Player2Name.Text = "Игрок 2";
            gameTimer.Start(); // Запуск таймера

            UpdatePlayerPositions();
            UpdateMiddleLine();
        }

        private void UpdatePlayerPositions()
        {
            // Обновление позиций игроков по вертикали
            double canvasHeight = GameCanvas.ActualHeight;
            double playerHeight = Player1.Height;
            Canvas.SetTop(Player1, (canvasHeight - playerHeight) / 2);
            Canvas.SetTop(Player2, (canvasHeight - playerHeight) / 2);
            Canvas.SetLeft(Player1, 20);
            Canvas.SetLeft(Player2, GameCanvas.ActualWidth - Player2.Width - 20);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Обновляем позиции игроков и сетки при изменении размера окна
            UpdatePlayerPositions();
            UpdateMiddleLine();
        }

        private void UpdateMiddleLine()
        {
            // Установка позиции и размера центральной линии
            double canvasHeight = GameCanvas.ActualHeight;
            Canvas.SetLeft(MiddleLine, (GameCanvas.ActualWidth - MiddleLine.Width) / 2);
            Canvas.SetTop(MiddleLine, 0);
            MiddleLine.Height = canvasHeight;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Начало игры
            StartGame();
        }

        private void StartGame()
        {
            player1Score = 0; // Сброс счета игрока 1
            player2Score = 0; // Сброс счета игрока 2
            UpdateScores(); // Обновление счета на экране
            ResetBall(new Random().Next(0, 2) == 0); // Сброс мяча с случайным направлением
            gameTimer.Start(); // Запуск таймера

            UpdateMiddleLine(); // Обновляем позицию сетки
            // Скрытие кнопки "Начать игру" и установка фокуса на игровом холсте
            StartButton.Visibility = Visibility.Collapsed;
            GameCanvas.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W && isPlayer1Active)
                MovePlayer(Player1, -10); // Плавное движение вверх
            else if (e.Key == Key.S && isPlayer1Active)
                MovePlayer(Player1, 10);  // Плавное движение вниз
            else if (e.Key == Key.Up && isPlayer2Active)
                MovePlayer(Player2, -10); // Плавное движение вверх
            else if (e.Key == Key.Down && isPlayer2Active)
                MovePlayer(Player2, 10);  // Плавное движение вниз
        }



        private void MovePlayer(Rectangle player, double offset)
        {
            // Перемещение игрока вверх или вниз
            double newTop = Canvas.GetTop(player) + offset;
            if (newTop < 0)
                newTop = 0;
            else if (newTop + player.Height > GameCanvas.ActualHeight)
                newTop = GameCanvas.ActualHeight - player.Height;
            Canvas.SetTop(player, newTop);
        }
    }
}

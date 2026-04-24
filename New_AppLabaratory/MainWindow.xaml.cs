using AppLaboratory;
using AppLaboratory.Labs;
using New_AppLabaratory.Buxg;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

namespace New_AppLabaratory
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private int failedLoginCount;
        public string captchaPublic;

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += WaitingEvent;
        }

        private void DisableField()
        {
            Login.IsEnabled = false;
            PasswordText.IsEnabled = false;
            Password.IsEnabled = false;
            captcha.IsEnabled = false;
            EnterSystem.IsEnabled = false;
            updateCaptcha.IsEnabled = false;
        }

        private void EnabledField()
        {
            Login.IsEnabled = true;
            PasswordText.IsEnabled = true;
            Password.IsEnabled = true;
            captcha.IsEnabled = true;
            updateCaptcha.IsEnabled = true;
            EnterSystem.IsEnabled = true;
        }

        private void SetTimer()
        {
            timer.Stop();
            timer.Start();
        }

        private void SetCaptcha()
        {
            captcha.Clear();
            captchaPublic = CaptchaClass.GenerateCaptcha();
            CanvasCaptcha.Children.Clear();

            Random random = new Random();
            CanvasCaptcha.Background = new SolidColorBrush(Color.FromRgb(
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256),
                (byte)random.Next(0, 256)));

            for (int i = 0; i < captchaPublic.Length; i++)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = captchaPublic[i].ToString(),
                    FontSize = random.Next(20, 30),
                    Foreground = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256),
                        (byte)random.Next(0, 256))),
                    RenderTransform = new RotateTransform(random.Next(-5, 5)),
                    Margin = new Thickness(i * 50, 0, 0, 0)
                };

                CanvasCaptcha.Children.Add(textBlock);
            }
        }

        public void WaitingEvent(object source, EventArgs e)
        {
            EnabledField();
            timer.Stop();
        }

        private void EnterSystem_Click(object sender, RoutedEventArgs e)
        {
            string login = Login.Text.Trim();
            string password = Password.Visibility == Visibility.Visible
                ? Password.Password
                : PasswordText.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните поля логина и пароля.");
                return;
            }

            if (failedLoginCount > 0 && !IsCaptchaValid())
            {
                MessageBox.Show("Капча введена неверно. Подождите 10 секунд и попробуйте снова.");
                SetTimer();
                DisableField();
                SetCaptcha();
                return;
            }

            int userType;
            if (!SQLClass.TryAuthenticateUser(login, password, out userType))
            {
                MessageBox.Show("Пароль или логин введены неверно!");
                SQLClass.errorSignIn(login);
                failedLoginCount++;
                EnsureCaptchaVisible();
                SetCaptcha();
                return;
            }

            SQLClass.succesSignIn(login);
            OpenWindowByRole(login, userType);
        }

        private bool IsCaptchaValid()
        {
            return string.Equals(captcha.Text.Trim(), captchaPublic, StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureCaptchaVisible()
        {
            if (GridCaptcha.Visibility != Visibility.Visible)
            {
                GridCaptcha.Visibility = Visibility.Visible;
            }
        }

        private void OpenWindowByRole(string login, int userType)
        {
            Window nextWindow;
            string roleName;

            switch (userType)
            {
                case 1:
                    roleName = "Админ";
                    nextWindow = new Administrator(login, userType);
                    break;
                case 2:
                    roleName = "Бухгалтер";
                    nextWindow = new Buxgalter(login, userType);
                    break;
                case 3:
                    roleName = "Лаборант";
                    nextWindow = new Lab(login, userType);
                    break;
                case 4:
                    roleName = "Лаборант-Исследователь";
                    nextWindow = new Lab_res(login, userType);
                    break;
                default:
                    MessageBox.Show("Для пользователя не определена роль.");
                    return;
            }

            MessageBox.Show($"Вы вошли как {roleName}!");
            nextWindow.Show();
            Close();
        }

        private void CheckPass_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPass.IsChecked == true)
            {
                PasswordText.Text = Password.Password;
                PasswordText.Visibility = Visibility.Visible;
                Password.Visibility = Visibility.Hidden;
                return;
            }

            Password.Password = PasswordText.Text;
            PasswordText.Visibility = Visibility.Hidden;
            Password.Visibility = Visibility.Visible;
        }

        private void updateCaptcha_Click(object sender, RoutedEventArgs e)
        {
            SetCaptcha();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}

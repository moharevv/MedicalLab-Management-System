using AppLaboratory;
using AppLaboratory.Labs;
using New_AppLabaratory.Buxg;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using System.Windows.Threading;

namespace New_AppLabaratory
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public int Count = 0;// счетчик входов в прогу
        public MainWindow()
        {
            InitializeComponent();
            Login.Text = "lab";
            Password.Password = "lab";
        }

        private void DisableField() // метод блокировка полей 
        {
            Login.IsEnabled = false;
            PasswordText.IsEnabled = false;
            Password.IsEnabled = false;
            captcha.IsEnabled = false;
            EnterSystem.IsEnabled = false;
            updateCaptcha.IsEnabled = false;
        }

        private void EnabledField() //метод разблоктровки полей
        {
            Login.IsEnabled = true;
            Login.IsEnabled = true;
            PasswordText.IsEnabled = true;
            Password.IsEnabled = true;
            captcha.IsEnabled = true;
            updateCaptcha.IsEnabled = true;
            EnterSystem.IsEnabled = true;
        }

        private void SetTimer() // метод создания и запуска таймер
        {
            timer.Tick += new EventHandler(WaitingEvent);
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start(); // таймер

        }

        public string captchaPublic;

        private void SetCaptcha()
        {
            captcha.Clear();// очищаю тб с капчей
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

        public void WaitingEvent(object Source, EventArgs e) // метод который выпооняет действия после окончания времени таймера
        {
            EnabledField();
            timer.Stop();
        }

        private void EnterSystem_Click(object sender, RoutedEventArgs e)
        {
            if (Login.Text != "" && Password.Password != "")
            {
                if (SQLClass.Enter(Login.Text, Password.Password) == true)
                {
                    if (Count > 0) // если входит со второй попытки то проверяем капчу
                    {
                        if (captcha.Text == captchaPublic)
                        {
                            if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 4)
                            {
                                MessageBox.Show("Вы вошли как Лаборант-Исследователь!");

                                //SQLClass.succesSignIn(Login.Text);

                                Lab_res lab_res = new Lab_res(Login.Text, 4);
                                lab_res.Show();
                                this.Close();
                            }
                            else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 3)
                            {
                                MessageBox.Show("Вы вошли как Лаборант!");

                                //SQLClass.succesSignIn(Login.Text);

                                Lab lab = new Lab(Login.Text, 3);
                                lab.Show();
                                this.Close();
                            }
                            else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 2)
                            {
                                MessageBox.Show("Вы вошли как Бухгалтер!");

                                // SQLClass.succesSignIn(Login.Text);

                                Buxgalter buxgalter = new Buxgalter(Login.Text, 2);
                                buxgalter.Show();
                                this.Close();
                            }
                            else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 1)
                            {
                                MessageBox.Show("Вы вошли как Админ!");
                                //SQLClass.succesSignIn(Login.Text);

                                Administrator admin = new Administrator(Login.Text, 1);
                                admin.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Капча введена невверно! \n Ждите 10 секунд");
                            SetTimer();// ставим таймер на 10 сек
                            DisableField(); // block field
                            SetCaptcha(); // update capacha
                        }
                    }
                    else
                    {
                        if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 4)
                        {
                            MessageBox.Show("Вы вошли как Лаборант-Исследователь!");
                            //SQLClass.succesSignIn(Login.Text);
                            Lab_res lab_res = new Lab_res(Login.Text, 4);
                            lab_res.Show();
                            this.Close();
                        }
                        else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 3)
                        {
                            MessageBox.Show("Вы вошли как Лаборант!");
                            //SQLClass.succesSignIn(Login.Text);
                            Lab lab = new Lab(Login.Text, 3);
                            lab.Show();
                            this.Close();
                        }
                        else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 2)
                        {
                            MessageBox.Show("Вы вошли как Бухгалтер!");
                            //SQLClass.succesSignIn(Login.Text);
                            Buxgalter buxgalter = new Buxgalter(Login.Text, 2);
                            buxgalter.Show();
                            this.Close();
                        }
                        else if (SQLClass.UsersCheckType(Login.Text, Password.Password) == 1)
                        {
                            MessageBox.Show("Вы вошли как Админ!");
                            SQLClass.succesSignIn(Login.Text);
                            Administrator admin = new Administrator(Login.Text, 1);
                            admin.Show();
                            this.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пароль или логин введены неверно!");
                    SQLClass.errorSignIn(Login.Text);// запись в журнал авторизаций
                    Count++;
                    if (Count == 1) //это чтобы капча один раз создалась а потом тольок по кнопке обновить обновляляась
                    {
                        if (GridCaptcha.Visibility != Visibility.Visible)
                        {
                            GridCaptcha.Visibility = Visibility.Visible;
                        }

                        SetCaptcha(); // тут будем создавать капчу
                    }
                    else if (Count > 1)
                    {
                        SetCaptcha();
                    }
                }
                SQLClass.CloseConnection();
            }
            else
            {
                MessageBox.Show("Заполните поля!");
            }
        }

        private void CheckPass_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPass.IsChecked.Value)
            {
                PasswordText.Text = Password.Password; // скопируем в TextBox из PasswordBox
                PasswordText.Visibility = Visibility.Visible; // TextBox - отобразить
                Password.Visibility = Visibility.Hidden; // PasswordBox - скрыть
            }
            else if (CheckPass.IsEnabled)
            {
                Password.Password = PasswordText.Text; // скопируем в PasswordBox из TextBox 
                PasswordText.Visibility = Visibility.Hidden; // TextBox - скрыть
                Password.Visibility = Visibility.Visible; // PasswordBox - отобразить
            }
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

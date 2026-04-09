using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient; // обязательная библиотека классов для работы с sql server
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_AppLabaratory
{
    internal class SQLClass
    {
        public static SqlConnection str; // перменная для подключения 

        public static void OpenConnection() // метод подключения к базе данных
        {
            str = new SqlConnection
            {
                ConnectionString = // строка подключения к бд
                // в папке проекта должно быть 2 файла с расширением .mdf и .log
@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=C:\Users\moh\Desktop\Projects\New_AppLabaratory\BASE_LABORATORY.MDF;Integrated Security=True"
                //@"Data Source=DESKTOP-HV9HRVA\SQLEXPRESS19;Initial Catalog=FamilyBudget;Integrated Security=True"
            };
            str.Open(); // открытие соединения
        }

        public static void CloseConnection() // метод закрытия соединения с бд
        {
            str.Close(); // закрытие соединения
        }

        public static List<string> Select(String Text, SqlConnection str) // метод запроса данных из бд
        {
            List<string> results = new List<string>(); // объявление массива где будут хранится данные возвращаемые sql запросом 
            SqlCommand command = new SqlCommand(Text, str); // запись sql команды

            SqlDataReader reader = command.ExecuteReader(); // объявление readera для учета данных получаемых из бд

            while (reader.Read()) // запуск цикла среди всех значений полученных запросом из бд
            {
                for (int i = 0; i < reader.FieldCount; i++) // запуск цикла для присвоения значений в массив данных
                    results.Add(reader.GetValue(i).ToString());
            }
            reader.Close(); // закрытие readrea 
            command.Dispose();

            return results; // возврат значений метода
        }
        public class user
        {
            public static string login { get; set; }
            public static string data { get; set; }
            public static string succes { get; set; }
        }

        public static DataTable ExecuteSql(string sql)
        {
            DataTable dt = new DataTable();
            // строка подключения к бд
            // в папке проекта должно быть 2 файла с расширением .mdf и .log
            string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;
            AttachDbFilename=C:\Users\moh\Desktop\Projects\New_AppLabaratory\BASE_LABORATORY.MDF;
            Integrated Security=True";

            SqlConnection conn = new SqlConnection(ConnectionString);

            using (conn)
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader read = cmd.ExecuteReader();

                using (read)
                {
                    dt.Load(read);
                }
            }
            return dt;
        }

        public static void Delete(String Text) // метод для удаления строки из бд
        {
            SqlCommand command = new SqlCommand(Text, str); // запрос на удаление
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public static void Update(string codeUslugi)
        {

        }

        public static bool Enter(string login, string passwordLog)
        {
            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("SELECT COUNT(*) from [dbo].[users] where login = @uL AND password = @uP", SQLClass.str);
            command.Parameters.Add("@uL", SqlDbType.VarChar).Value = login;
            command.Parameters.Add("@uP", SqlDbType.VarChar).Value = passwordLog;
            var result = command.ExecuteScalar() as int?;
            SQLClass.CloseConnection();
            return result == 1;
        }
        public static int GetLastOrderId()
        {
            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("select MAX(\"id\") from Заказ", SQLClass.str);
            int lastid = (int)command.ExecuteScalar();
            return lastid;
        }
        public static string GetName(string login, int type)
        {
            SQLClass.OpenConnection();
            //var command = new SqlCommand(Query, Connection);
            SqlCommand cmd = new SqlCommand("SELECT name from[dbo].[users] where login = @uL and type = @type", SQLClass.str);
            cmd.Parameters.Add("@uL", SqlDbType.VarChar).Value = login;
            cmd.Parameters.Add("@type", SqlDbType.VarChar).Value = type;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            string name = reader["name"].ToString();
            SQLClass.CloseConnection();
            return name;
        }


        //public static int EnterAdmin(string login, string passwordLog)
        //{
        //    int otv = 1;
        //    var CurrentUser = AppData.db.users.FirstOrDefault(u => u.login == login && u.password == passwordLog && u.type == 1);

        //    if (CurrentUser != null)
        //    {
        //        return otv;
        //    }
        //    else
        //    {
        //        otv = 0;
        //        return otv;
        //    }

        //}
        public static int UsersCheckType(string login, string passwordLog)
        {
            SQLClass.OpenConnection();
            //var command = new SqlCommand(Query, Connection);
            SqlCommand cmd = new SqlCommand("SELECT type from[dbo].[users] where login = @log and password = @pass", SQLClass.str);
            cmd.Parameters.Add("@log", SqlDbType.VarChar).Value = login;
            cmd.Parameters.Add("@pass", SqlDbType.VarChar).Value = passwordLog;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int type = Convert.ToInt32(reader["type"]);
            SQLClass.CloseConnection();
            return type;
        }

        //public static int EnterBuxgalter(string login, string passwordLog)
        //{
        //    int otv = 2;
        //    var CurrentUser = AppData.db.users.FirstOrDefault(u => u.login == login && u.password == passwordLog && u.type == 2);
        //    if (CurrentUser != null)
        //    {
        //        return otv;
        //    }
        //    else
        //    {
        //        otv = 0;
        //        return otv;
        //    }
        //}
        //public static int EnterBuxgalter1(string login, string passwordLog)
        //{
        //    SQLClass.OpenConnection();
        //    SqlCommand cmd = new SqlCommand("SELECT type from[dbo].[users] where login = @log and password = @pass", SQLClass.str);
        //    cmd.Parameters.Add("@log", SqlDbType.VarChar).Value = login;
        //    cmd.Parameters.Add("@pass", SqlDbType.VarChar).Value = passwordLog;
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    reader.Read();
        //    int type = Convert.ToInt32(reader["type"]);
        //    SQLClass.CloseConnection();
        //    return type;
        //}
        //public static int EnterLaborant(string login, string passwordLog)
        //{
        //    int otv = 3;
        //    var CurrentUser = AppData.db.users.FirstOrDefault(u => u.login == login && u.password == passwordLog && u.type == 3);
        //    if (CurrentUser != null)
        //    {
        //        return otv;
        //    }
        //    else
        //    {
        //        otv = 0;
        //        return otv;
        //    }
        //}
        public static void succesSignIn(string login)
        {
            string username = login; // имя пользователя
            DateTime timestamp = DateTime.Now; // текущее время
            string success = "Успешно"; // успешность авторизации

            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("INSERT INTO AuthHistory (Username, Date, EnterSucces) VALUES (@Username, @Timestamp, @Success)", SQLClass.str);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Timestamp", timestamp);
            command.Parameters.AddWithValue("@Success", success);
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();

        }
        public static void errorSignIn(string login)
        {
            string username = login; // имя пользователя
            DateTime timestamp = DateTime.Now; // текущее время
            string success = "Неуспешно"; // успешность авторизации

            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("INSERT INTO AuthHistory (UserName, Date, EnterSucces) VALUES (@Username, @Timestamp, @Success)", SQLClass.str);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Timestamp", timestamp);
            command.Parameters.AddWithValue("@Success", success);
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();
        }
        public static void InsertInOrder(int biomaterial, int pacient, int service)
        {
            DateTime timestamp = DateTime.Now; // дата заказа
            int services = service; // id услуги(тип услуги)
            string statusOrder = "Выполнено";
            string statusService = "Ожидание";
            string timeComplete = "30 секунд";

            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("INSERT INTO Заказ (Дата_создания, Тип_услуги, Статус_заказа, Статус_услуги_в_заказе, Время_выполнения, id_пациента, id_биоматериала) VALUES (@Дата_создания, @Тип_услуги, @Статус_заказа, @Статус_услуги_в_заказе, @Время_выполнения, @id_пациента, @id_биоматериала)", SQLClass.str);
            command.Parameters.AddWithValue("@Дата_создания", timestamp);
            command.Parameters.AddWithValue("@Тип_услуги", service);
            command.Parameters.AddWithValue("@Статус_заказа", statusOrder);
            command.Parameters.AddWithValue("@Статус_услуги_в_заказе", statusService);
            command.Parameters.AddWithValue("@Время_выполнения", timeComplete);
            command.Parameters.AddWithValue("@id_пациента", pacient);
            command.Parameters.AddWithValue("@id_биоматериала", biomaterial);
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();
        }
        public static void InsertServicesInOrder(int id_service, int id_order)
        {
            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("INSERT INTO Услуга_В_заказе (id_Услуги, id_Заказа, Статус) VALUES (@Id_Service, @Id_Order, @Status)", SQLClass.str);
            command.Parameters.AddWithValue("@Id_Service", id_service);
            command.Parameters.AddWithValue("@Id_Order", id_order);
            command.Parameters.AddWithValue("@Status", "Ожидание");
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();
        }

        public static bool BarcodeInDB(string barcode)
        {
            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("SELECT COUNT(*) from Биоматериал where Код = @barcode", SQLClass.str);
            command.Parameters.Add("@barcode", SqlDbType.VarChar).Value = barcode;
            var result = command.ExecuteScalar() as int?;
            SQLClass.CloseConnection();
            return result == 1;
        }

        public static int GetIdService(string service)
        {
            SQLClass.OpenConnection();
            //var command = new SqlCommand(Query, Connection);
            SqlCommand cmd = new SqlCommand("SELECT Code from services where Service = @S", SQLClass.str);
            cmd.Parameters.Add("@S", SqlDbType.VarChar).Value = service;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int Code = Convert.ToInt32(reader["Code"]);
            SQLClass.CloseConnection();
            return Code;
        }
        // запрос на поиск id где он одинаковый в таблицах
        public static int GetId(string querry, string value)
        {
            SQLClass.OpenConnection();
            SqlCommand cmd = new SqlCommand(querry, SQLClass.str);
            cmd.Parameters.Add("@Value", SqlDbType.VarChar).Value = value;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int id = Convert.ToInt32(reader["id"]);
            SQLClass.CloseConnection();
            return id;
        }

        public static int GetIdPIZDEC(string service)
        {
            SQLClass.OpenConnection();
            SqlCommand cmd = new SqlCommand("select id_пациента\r\nfrom Заказ\r\ninner join Услуга_В_заказе U\r\non Заказ.id = U.id_Заказа\r\ninner join services\r\non services.Code = U.id_Услуги where services.Service = @service and U.Статус = 'Ожидание' and Analyser = '1' or Analyser = '1;2';", SQLClass.str);
            cmd.Parameters.Add("@service", SqlDbType.VarChar).Value = service;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int id = Convert.ToInt32(reader["id_пациента"]);
            SQLClass.CloseConnection();
            return id;
        }

        public static int GetIdPacient(string FIO)
        {
            SQLClass.OpenConnection();
            SqlCommand cmd = new SqlCommand("SELECT id from Данные_пациентов where ФИО = @FIO", SQLClass.str);
            cmd.Parameters.Add("@FIO", SqlDbType.VarChar).Value = FIO;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            int id = Convert.ToInt32(reader["id"]);
            SQLClass.CloseConnection();
            return id;
        }
        public static int GetIdBIO(string codeBio, string nameBio)
        {
            try
            {
                SQLClass.OpenConnection();
                SqlCommand cmd = new SqlCommand("SELECT id from Биоматериал where Код = @BIO", SQLClass.str);
                cmd.Parameters.Add("@BIO", SqlDbType.VarChar).Value = codeBio;
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                int id = Convert.ToInt32(reader["id"]);
                SQLClass.CloseConnection();
                return id;
            }
            catch (Exception)
            {

                //SQLClass.OpenConnection();
                //SqlCommand command = new SqlCommand("INSERT INTO Биоматериал (Код, Название) VALUES (@code, @bio)", SQLClass.str);
                //command.Parameters.AddWithValue("@code", codeBio);
                //command.Parameters.AddWithValue("@bio", nameBio);
                //command.ExecuteNonQuery();
                //SQLClass.CloseConnection();
                throw;
            }

        }
        /*
        public static string GetCodeProbirki(string BIO)
        {
            SQLClass.OpenConnection();
            SqlCommand cmd = new SqlCommand("SELECT Код from Биоматериал where Название = @BIO", SQLClass.str);
            cmd.Parameters.Add("@BIO", SqlDbType.VarChar).Value = BIO;
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            string code = reader["Код"].ToString();
            SQLClass.CloseConnection();
            return code;
        }
        */
        public static void WrtieBioBD(string code, string biomaterial)
        {
            SQLClass.OpenConnection();
            SqlCommand command = new SqlCommand("INSERT INTO Биоматериал (Код, Название) VALUES (@code, @bio)", SQLClass.str);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@bio", biomaterial);
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();
        }
        public static void WritePacientBD(string Name, string born, string passport, string phone, string email, string polis, int typePolis, int company)
        {
            SQLClass.OpenConnection();
            string login = "login1";
            string password = "pass1";
            DateTime birthdate = DateTime.ParseExact(born, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            SqlCommand command = new SqlCommand("insert into Данные_пациентов (Логин, Пароль, ФИО, Дата_рождения, Серия_и_номер_паспорта, Телефон, Email, Номер_полиса, Тип_полиса, Страховая_компания) values (@login, @password, @Name, @birthdate, @passport, @phone, @email, @numPolis, @typePolis, @company)", SQLClass.str);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@Name", Name);
            command.Parameters.AddWithValue("@birthdate", birthdate);
            command.Parameters.AddWithValue("@passport", passport);
            command.Parameters.AddWithValue("@phone", phone);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@numPolis", polis);
            command.Parameters.AddWithValue("@typePolis", typePolis);
            command.Parameters.AddWithValue("@company", company);
            command.ExecuteNonQuery();
            SQLClass.CloseConnection();
        }
    }
}

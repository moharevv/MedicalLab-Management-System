using New_AppLabaratory.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace New_AppLabaratory
{
    internal class SQLClass
    {
        private const string ConnectionString =
            @"Data Source=(LocalDB)\MSSQLLocalDB;
            AttachDbFilename=|DataDirectory|\Database\BASE_LABORATORY.mdf;
            Integrated Security=True";

        public static SqlConnection str;

        public static void OpenConnection()
        {
            if (str != null && str.State == ConnectionState.Open)
            {
                return;
            }

            CloseConnection();
            str = new SqlConnection(ConnectionString);
            str.Open();
        }

        public static void CloseConnection()
        {
            if (str == null)
            {
                return;
            }

            if (str.State != ConnectionState.Closed)
            {
                str.Close();
            }

            str.Dispose();
            str = null;
        }

        private static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        private static SqlConnection GetConnection(out bool shouldDispose)
        {
            if (str != null && str.State == ConnectionState.Open)
            {
                shouldDispose = false;
                return str;
            }

            SqlConnection connection = CreateConnection();
            connection.Open();
            shouldDispose = true;
            return connection;
        }

        private static void DisposeConnection(SqlConnection connection, bool shouldDispose)
        {
            if (shouldDispose && connection != null)
            {
                connection.Dispose();
            }
        }

        public static List<Service> GetServicesList(SqlConnection connection)
        {
            List<Service> services = new List<Service>();
            string query = "SELECT Code, Service, Price FROM services";

            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    services.Add(new Service
                    {
                        Id = Convert.ToInt32(reader["Code"]),
                        Name = reader["Service"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"])
                    });
                }
            }

            return services;
        }

        public static List<string> Select(string text, SqlConnection connection)
        {
            List<string> results = new List<string>();

            using (SqlCommand command = new SqlCommand(text, connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        results.Add(reader.GetValue(i).ToString());
                    }
                }
            }

            return results;
        }

        public static DataTable ExecuteSql(string sql)
        {
            DataTable dt = new DataTable();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            bool shouldDispose;
            SqlConnection connection = GetConnection(out shouldDispose);

            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    return command.ExecuteNonQuery();
                }
            }
            finally
            {
                DisposeConnection(connection, shouldDispose);
            }
        }

        public static void Delete(string text)
        {
            ExecuteNonQuery(text);
        }

        public static bool TryAuthenticateUser(string login, string passwordLog, out int userType)
        {
            userType = 0;

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT type FROM [dbo].[users] WHERE login = @login AND password = @password",
                connection))
            {
                connection.Open();
                command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
                command.Parameters.Add("@password", SqlDbType.VarChar).Value = passwordLog;

                object result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    return false;
                }

                userType = Convert.ToInt32(result);
                return true;
            }
        }

        public static bool Enter(string login, string passwordLog)
        {
            int userType;
            return TryAuthenticateUser(login, passwordLog, out userType);
        }

        public static int GetLastOrderId()
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT ISNULL(MAX([id]), 0) FROM Заказ", connection))
            {
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static string GetName(string login, int type)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT name FROM [dbo].[users] WHERE login = @login AND type = @type",
                connection))
            {
                connection.Open();
                command.Parameters.Add("@login", SqlDbType.VarChar).Value = login;
                command.Parameters.Add("@type", SqlDbType.Int).Value = type;

                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? string.Empty : result.ToString();
            }
        }

        public static int UsersCheckType(string login, string passwordLog)
        {
            int userType;
            return TryAuthenticateUser(login, passwordLog, out userType) ? userType : 0;
        }

        public static void succesSignIn(string login)
        {
            ExecuteNonQuery(
                "INSERT INTO AuthHistory (Username, Date, EnterSucces) VALUES (@Username, @Timestamp, @Success)",
                new SqlParameter("@Username", login),
                new SqlParameter("@Timestamp", DateTime.Now),
                new SqlParameter("@Success", "Успешно"));
        }

        public static void errorSignIn(string login)
        {
            ExecuteNonQuery(
                "INSERT INTO AuthHistory (UserName, Date, EnterSucces) VALUES (@Username, @Timestamp, @Success)",
                new SqlParameter("@Username", login),
                new SqlParameter("@Timestamp", DateTime.Now),
                new SqlParameter("@Success", "Неуспешно"));
        }

        public static void InsertInOrder(int biomaterial, int pacient)
        {
            ExecuteNonQuery(
                "INSERT INTO Заказ (Дата_создания, Статус_заказа, Время_выполнения, id_пациента, id_биоматериала) " +
                "VALUES (@Дата_создания, @Статус_заказа, @Время_выполнения, @id_пациента, @id_биоматериала)",
                new SqlParameter("@Дата_создания", DateTime.Now),
                new SqlParameter("@Статус_заказа", "Создано"),
                new SqlParameter("@Время_выполнения", "30 секунд"),
                new SqlParameter("@id_пациента", pacient),
                new SqlParameter("@id_биоматериала", biomaterial));
        }

        public static void InsertServicesInOrder(int id_service, int id_order)
        {
            ExecuteNonQuery(
                "INSERT INTO Услуга_В_заказе (id_Услуги, id_Заказа, Статус) VALUES (@Id_Service, @Id_Order, @Status)",
                new SqlParameter("@Id_Service", id_service),
                new SqlParameter("@Id_Order", id_order),
                new SqlParameter("@Status", "Ожидание"));
        }

        public static bool BarcodeInDB(string barcode)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Биоматериал WHERE Код = @barcode", connection))
            {
                connection.Open();
                command.Parameters.Add("@barcode", SqlDbType.VarChar).Value = barcode;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        public static int GetIdService(string service)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT Code FROM services WHERE Service = @service", connection))
            {
                connection.Open();
                command.Parameters.Add("@service", SqlDbType.VarChar).Value = service;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public static int GetId(string querry, string value)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(querry, connection))
            {
                connection.Open();
                command.Parameters.Add("@Value", SqlDbType.VarChar).Value = value;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return 0;
                    }

                    return Convert.ToInt32(reader["id"]);
                }
            }
        }

        public static int GetPatientIdForPendingAnalyzerService(string service)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT TOP 1 Заказ.id_пациента " +
                "FROM Заказ " +
                "INNER JOIN Услуга_В_заказе U ON Заказ.id = U.id_Заказа " +
                "INNER JOIN services ON services.Code = U.id_Услуги " +
                "WHERE services.Service = @service " +
                "AND U.Статус = N'Ожидание' " +
                "AND (Analyser = '1' OR Analyser = '1;2');",
                connection))
            {
                connection.Open();
                command.Parameters.Add("@service", SqlDbType.VarChar).Value = service;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public static int GetIdPacient(string fio)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT id FROM Данные_пациентов WHERE ФИО = @fio", connection))
            {
                connection.Open();
                command.Parameters.Add("@fio", SqlDbType.VarChar).Value = fio;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public static int GetIdBIO(string codeBio, string nameBio)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand("SELECT id FROM Биоматериал WHERE Код = @bioCode", connection))
            {
                connection.Open();
                command.Parameters.Add("@bioCode", SqlDbType.VarChar).Value = codeBio;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public static void WriteBiomaterial(string code, string biomaterial)
        {
            ExecuteNonQuery(
                "INSERT INTO Биоматериал (Код, Название) VALUES (@code, @bio)",
                new SqlParameter("@code", code),
                new SqlParameter("@bio", biomaterial));
        }

        public static void WrtieBioBD(string code, string biomaterial)
        {
            WriteBiomaterial(code, biomaterial);
        }

        public static void WritePacientBD(string name, string born, string passport, string phone, string email, string polis, int typePolis, int company)
        {
            DateTime birthdate = DateTime.ParseExact(born, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            string generatedLogin = "patient_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string generatedPassword = Guid.NewGuid().ToString("N").Substring(0, 10);

            ExecuteNonQuery(
                "INSERT INTO Данные_пациентов (Логин, Пароль, ФИО, Дата_рождения, Серия_и_номер_паспорта, Телефон, Email, Номер_полиса, Тип_полиса, Страховая_компания) " +
                "VALUES (@login, @password, @Name, @birthdate, @passport, @phone, @email, @numPolis, @typePolis, @company)",
                new SqlParameter("@login", generatedLogin),
                new SqlParameter("@password", generatedPassword),
                new SqlParameter("@Name", name),
                new SqlParameter("@birthdate", birthdate),
                new SqlParameter("@passport", passport),
                new SqlParameter("@phone", phone),
                new SqlParameter("@email", email),
                new SqlParameter("@numPolis", polis),
                new SqlParameter("@typePolis", typePolis),
                new SqlParameter("@company", company));
        }

        public static void UpdateServiceStatus(int orderId, int serviceCode, string newStatus)
        {
            ExecuteNonQuery(
                "UPDATE Услуга_В_заказе SET Статус = @status WHERE id_Заказа = @orderId AND id_Услуги = @serviceCode",
                new SqlParameter("@status", newStatus),
                new SqlParameter("@orderId", orderId),
                new SqlParameter("@serviceCode", serviceCode));
        }

        public static void RejectServiceAndClearResult(int orderId, int serviceCode, string newStatus)
        {
            ExecuteNonQuery(
                "UPDATE Услуга_В_заказе SET Статус = @status, Result = NULL WHERE id_Заказа = @orderId AND id_Услуги = @serviceCode",
                new SqlParameter("@status", newStatus),
                new SqlParameter("@orderId", orderId),
                new SqlParameter("@serviceCode", serviceCode));
        }

        public static double GetAverageResult(int serviceCode)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(
                "SELECT AVG(CAST(Result AS float)) FROM Услуга_В_заказе WHERE id_Услуги = @serviceCode AND Статус = N'Выполнена'",
                connection))
            {
                connection.Open();
                command.Parameters.Add("@serviceCode", SqlDbType.Int).Value = serviceCode;
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToDouble(result);
            }
        }

        public static void SaveServiceResult(int orderId, int serviceCode, string resultValue)
        {
            ExecuteNonQuery(
                "UPDATE Услуга_В_заказе SET Result = @res WHERE id_Заказа = @oid AND id_Услуги = @sid",
                new SqlParameter("@res", resultValue),
                new SqlParameter("@oid", orderId),
                new SqlParameter("@sid", serviceCode));
        }
    }
}

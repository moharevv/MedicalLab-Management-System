using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_AppLabaratory.Classes
{
    public class Service
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public decimal Price { get; set; }

        // --- Добавляем поля для работы с анализатором ---

        public string Status { get; set; }    // "Ожидание", "Отправлена на исследование", "Выполнена"
        public string Result { get; set; }    // Сюда запишем ответ от API (число или текст)
        public int OrderId { get; set; }      // Чтобы знать, какой заказ обновлять в базе

        // Поле для сопоставления с JSON (в задании API возвращает 'code')
        // Если используешь JavaScriptSerializer, он сопоставит это автоматически, 
        // если в JSON поле называется 'Id' или 'id'. 
        // Но в задании от API приходит 'code', поэтому добавим это свойство для совместимости.
        public int serviceCode { get => Id; set => Id = value; }
        public int code { get => Id; set => Id = value; }
    }
}

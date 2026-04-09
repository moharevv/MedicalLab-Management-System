select id_Услуги, id_Заказа, Статус
FROM Услуга_В_заказе
inner join services
on id_Услуги = services.Code where Analyser = '1' or Analyser = '1;2'
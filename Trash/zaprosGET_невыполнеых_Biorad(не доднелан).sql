select U.id_Услуги, U.id_Заказа, U.Статус 
from Услуга_В_заказе U
join services as S
on (S.Analyser = '1' or S.Analyser = '1;2' )
join services as S2
on S2.Code = U.id_Услуги
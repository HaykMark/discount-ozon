namespace Discounting.Entities.Extensions
{
    public static class SupplyTypeExtension
    {
        public static SupplyType GetType(string valueRus)
        {
            return valueRus.ToLower() switch
            {
                "акт" => SupplyType.Akt,
                "счёт-фактура" => SupplyType.Invoice,
                "счет-фактура" => SupplyType.Invoice,
                "торг-12" => SupplyType.Torg12,
                "укд" => SupplyType.Ukd,
                "упд" => SupplyType.Upd,
                _ => SupplyType.None
            };
        }
        
        public static string ToRussianName(this SupplyType type)
        {
            return type switch
            {
                SupplyType.Torg12 => "ТОРГ-12",
                SupplyType.Upd => "УПД",
                SupplyType.Akt => "АКТ",
                SupplyType.Invoice => "СЧЁТ-ФАКТУРА",
                SupplyType.Ukd => "УКД",
                _ => ""
            };
        }
    }
}
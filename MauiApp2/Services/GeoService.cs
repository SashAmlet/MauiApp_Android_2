using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2.Services;

public static class GeoService
{
    public static Location GetCoordinatesFromAddress(string address)
    {
        // Заглушка: тут ви можете використовувати Google Maps API або будь-який інший сервіс
        return address switch
        {
            "Київ, вул. Хрещатик, 1" => new Location(50.4501, 30.5234),
            "Одеса, вул. Дерибасівська, 10" => new Location(46.4825, 30.7233),
            _ => new Location(0, 0)
        };
    }
}

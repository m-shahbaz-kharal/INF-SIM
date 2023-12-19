using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static List<int> ParseTime(float seconds)
    {
        int years = (int) (seconds / 31536000.0f);
        seconds -= years * 31536000;
        int months =  (int) (seconds / 2592000.0f);
        seconds -= months * 2592000;
        int days =  (int) (seconds / 86400.0f);
        seconds -= days * 86400;
        int hours =  (int) (seconds / 3600.0f);
        seconds -= hours * 3600;
        int minutes =  (int) (seconds / 60.0f);
        seconds -= minutes * 60;
        return new List<int> { years, months, days, hours, minutes, (int)seconds };
    }
    public static string HumanTime(float seconds)
    {
        List<int> time = ParseTime(seconds);
        return $"{time[0]} years, {time[1]} months, {time[2]} days";
    }
}
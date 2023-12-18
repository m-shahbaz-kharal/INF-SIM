using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static List<int> ParseTime(int seconds)
    {
        int years = seconds / 31536000;
        seconds -= years * 31536000;
        int months = seconds / 2592000;
        seconds -= months * 2592000;
        int days = seconds / 86400;
        seconds -= days * 86400;
        int hours = seconds / 3600;
        seconds -= hours * 3600;
        int minutes = seconds / 60;
        seconds -= minutes * 60;
        return new List<int> { years, months, days, hours, minutes, seconds };
    }
    public static string HumanTime(float seconds)
    {
        List<int> time = ParseTime((int)seconds);
        return $"{time[0]} years, {time[1]} months, {time[2]} days";
    }
}
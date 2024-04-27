using System.Text.Json;

TemperatureLogger logger = new TemperatureLogger();
bool exit = false;

while (!exit)
{
    Console.WriteLine("-----------------------------------------");
    Console.WriteLine("Temperature Logger Menu:");
    Console.WriteLine("1. Log Temperature");
    Console.WriteLine("2. View Temperature for Specific Date");
    Console.WriteLine("3. View Temperature for Specific Week");
    Console.WriteLine("4. View Temperature for Specific Month");
    Console.WriteLine("5. Remove Temperature for Specific Date");
    Console.WriteLine("6. Exit");
    Console.Write("Enter your choice: ");

    if (int.TryParse(Console.ReadLine(), out int choice))
    {
        switch (choice)
        {
            case 1:
                LogTemperature(logger);
                break;
            case 2:
                ViewTemperature(logger);
                break;
            case 3:
                ViewTemperatureForWeek(logger);
                break;
            case 4:
                ViewTemperatureForMonth(logger);
                break;
            case 5:
                RemoveTemperature(logger);
                break;
            case 6:
                exit = true;
                break;
            default:
                Console.WriteLine("Invalid choice. Please try again.");
                Thread.Sleep(2000);
                break;
        }
    }
    else
    {
        Console.WriteLine("Invalid choice. Please enter a number.");
        Thread.Sleep(2000);
    }
}

static void LogTemperature(TemperatureLogger logger)
{
    Console.Write("Enter the city: ");
    string city = Console.ReadLine();
    Console.Write("Enter the date (YYYY-MM-DD): ");
    DateTime date = DateTime.Parse(Console.ReadLine());
    Console.Write("Enter the hour of day (0-23): ");
    int hour = int.Parse(Console.ReadLine());
    Console.Write("Enter the temperature in Celsius: ");
    int temperature = int.Parse(Console.ReadLine());
    logger.LogTemperature(city, date, hour, temperature);
    Console.WriteLine("Temperature logged successfully!");
    Thread.Sleep(2000);
}

static void ViewTemperature(TemperatureLogger logger)
{
    Console.Write("Enter the city: ");
    string city = Console.ReadLine();
    Console.Write("Enter the date to view temperature (YYYY-MM-DD): ");
    DateTime viewDate = DateTime.Parse(Console.ReadLine());
    logger.ViewTemperature(city, viewDate);
    Thread.Sleep(2000);
}

static void ViewTemperatureForWeek(TemperatureLogger logger)
{
    Console.Write("Enter the city: ");
    string city = Console.ReadLine();
    Console.Write("Enter the week to view temperature (YYYY-WW): ");
    string weekString = Console.ReadLine();
    logger.ViewTemperatureForWeek(city, weekString);
    Thread.Sleep(2000);
}

static void ViewTemperatureForMonth(TemperatureLogger logger)
{
    Console.Write("Enter the city: ");
    string city = Console.ReadLine();
    Console.Write("Enter the month to view temperature (YYYY-MM): ");
    string monthString = Console.ReadLine();
    logger.ViewTemperatureForMonth(city, monthString);
    Thread.Sleep(2000);
}

static void RemoveTemperature(TemperatureLogger logger)
{
    Console.Write("Enter the city: ");
    string city = Console.ReadLine();
    Console.Write("Enter the date to remove temperature (YYYY-MM-DD): ");
    DateTime removeDate = DateTime.Parse(Console.ReadLine());
    logger.RemoveTemperature(city, removeDate);
    Console.WriteLine("Temperature removed successfully!");
    Thread.Sleep(2000);
}

public class TemperatureLogger
{
    private Dictionary<string, Dictionary<DateTime, TemperatureEntry>> temperatureRecords;
    private string recordsFilePath = "temperature_records.json";

    public TemperatureLogger()
    {
        temperatureRecords = LoadTemperatureRecords();
    }

    public void LogTemperature(string city, DateTime date, int hour, int temperature)
    {
        if (!temperatureRecords.ContainsKey(city))
        {
            temperatureRecords[city] = new Dictionary<DateTime, TemperatureEntry>();
        }

        DateTime shortendDate = date.Date;

        if (!temperatureRecords[city].ContainsKey(shortendDate))
        {
            temperatureRecords[city][shortendDate] = new TemperatureEntry();
        }

        temperatureRecords[city][shortendDate].AddTemperature(hour, temperature);
        SaveTemperatureRecords();
    }

    public void ViewTemperature(string city, DateTime date)
    {
        if (temperatureRecords.ContainsKey(city) && temperatureRecords[city].ContainsKey(date.Date))
        {
            Console.WriteLine($"Temperature records for {city} on {date.ToShortDateString()}:");

            TemperatureEntry entry = temperatureRecords[city][date.Date];
            foreach (var hourTempPair in entry.TemperatureData)
            {
                Console.WriteLine($"Hour: {hourTempPair.Key}, Temperature: {hourTempPair.Value}°C");
            }
        }
        else
        {
            Console.WriteLine($"No temperature records found for {city} on {date.ToShortDateString()}");
        }
    }

    public void ViewTemperatureForWeek(string city, string weekString)
    {
        if (temperatureRecords.ContainsKey(city))
        {
            int year = int.Parse(weekString.Split('-')[0]);
            int week = int.Parse(weekString.Split('-')[1]);

            string formattedWeek = week < 10 ? $"0{week}" : week.ToString();

            DateTime startOfWeek = GetFirstDateOfWeek(year, week);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            Console.WriteLine($"Temperature records for {city} for week {formattedWeek} of {year}:");

            for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                ViewTemperature(city, date);
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"No temperature records found for {city}");
        }
    }

    public void ViewTemperatureForMonth(string city, string monthString)
    {
        if (temperatureRecords.ContainsKey(city))
        {
            int year = int.Parse(monthString.Split('-')[0]);
            int month = int.Parse(monthString.Split('-')[1]);
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            Console.WriteLine($"Temperature records for {city} for {startDate.ToString("MMMM yyyy")}:");
        
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                ViewTemperature(city, date);
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine($"No temperature records found for {city}");
        }
    }

    public void RemoveTemperature(string city, DateTime date)
    {
        if (temperatureRecords.ContainsKey(city) && temperatureRecords[city].ContainsKey(date.Date))
        {
            temperatureRecords[city].Remove(date.Date);
            SaveTemperatureRecords();
        }
        else
        {
            Console.WriteLine($"No temperature records found for {city} on {date.ToShortDateString()}");
        }
    }

    private Dictionary<string, Dictionary<DateTime, TemperatureEntry>> LoadTemperatureRecords()
    {
        if (File.Exists(recordsFilePath))
        {
            try
            {
                string json = File.ReadAllText(recordsFilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<Dictionary<string, Dictionary<DateTime, TemperatureEntry>>>(json);
                }
                else
                {
                    Console.WriteLine("JSON file is empty.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("JSON file does not exist.");
        }

        return new Dictionary<string, Dictionary<DateTime, TemperatureEntry>>();
    }

    private void SaveTemperatureRecords()
    {
        string json = JsonSerializer.Serialize(temperatureRecords);
        File.WriteAllText(recordsFilePath, json);
    }

    private DateTime GetFirstDateOfWeek(int year, int week)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = (int)DayOfWeek.Thursday - (int)jan1.DayOfWeek;

        DateTime firstThursday = jan1.AddDays(daysOffset);

        if (firstThursday.AddDays(-3).Year < year)
        {
            week -= 1;
        }

        DateTime result = firstThursday.AddDays((week - 1) * 7);

        return result.AddDays(-3);
    }
}

public class TemperatureEntry
{
    public Dictionary<int, int> TemperatureData { get; set; }

    public TemperatureEntry()
    {
        TemperatureData = new Dictionary<int, int>();
    }

    public void AddTemperature(int hour, int temperature)
    {
        TemperatureData[hour] = temperature;
    }
}
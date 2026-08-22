using System;
using System.ComponentModel;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


public class timeManager : MonoBehaviour
{

    [Header("Time State")]
    [SerializeField, Range(1, 31)] public int currentGameDay;
    [SerializeField, Range(0, 23)] public int currentGameHour;
    [SerializeField, Range(0, 59)] public int currentGameMinute;
    [SerializeField, Range(1, 59)] public int currentGameSecond;
    [SerializeField, Range(1, 12)] public int currentGameMonth;
    [SerializeField, Range(1, 7)] public int currentGameDayOfWeek;
    public int currentGameYear;

    [Header("Previous Time State")]
    [SerializeField, Range(1, 31)] public int previousGameDay;
    [SerializeField, Range(0, 23)] public int previousGameHour;
    [SerializeField, Range(0, 59)] public int previousGameMinute;
    [SerializeField, Range(1, 59)] public int previousGameSecond;
    [SerializeField, Range(1, 12)] public int previousGameMonth;
    [SerializeField, Range(1, 7)] public int previousGameDayOfWeek;

    public int previousGameYear;

    [Header("Last Save")]
    public string lastSave;
    [Header("First Save")]
    public string firstSave;


    [Header("Time Settings")]
    [SerializeField] int timeMultiplier = 3;
    [SerializeField] int sunriseHour = 7;
    [SerializeField] int sunsetHour = 19;
    [SerializeField] int hoursInDay = 24;
    [SerializeField] int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };


    [Header("Event Settings")]
    [SerializeField] private boolEvent onSunset;
    [SerializeField] private boolEvent onSunrise;
    [SerializeField] private intEvent onHourChange;
    [SerializeField] private intEvent onDayChange;
    [SerializeField] private intEvent onMonthChange;
    [SerializeField] private boolEvent onChangeToSummer;
    [SerializeField] private boolEvent onChangeToAutumn;
    [SerializeField] private boolEvent onChangeToWinter;
    [SerializeField] private boolEvent onChangeToSpring;
    [SerializeField] private intEvent onSunshineHourPassed;
    [SerializeField] private floatEvent dayPercentCompleted;
    [SerializeField] private stringEvent onDayChange_String;
    [SerializeField] private stringEvent onTimeChange_String;



    //state saver

    //timer
    private float secondTimer = 0.0f; //currently everything depends on this to make multipllying easier.


    void updateGameTimeString()
    {
        //placed this here because it's meaningfully changed at this point, in update is wasting compute
        dayPercentCompleted.Invoke(calculateDayPercentageCompleted());
        Debug.Log("dayPercentageCompleted invoked from updateGameTimeString");

        int hour = currentGameHour;
        int min = currentGameMinute;
        string suffix = hour >= 12 ? "pm" : "am";

        hour %= 12;
        if (hour == 0)
        {
            hour = 12;
        }

        onTimeChange_String.Invoke($"{hour}:{min:D2}{suffix}");
        Debug.Log("onTimeChange_String invoked from updateGameTimeString");
    }

    void updateGameDayString()
    {
        dayOfWeek day = (dayOfWeek)currentGameDayOfWeek;
        onDayChange_String.Invoke(ToDescriptionString(day));
        Debug.Log("onDayChange_String invoked from updateGameDayString");
    }


    void updateGameTime(float deltaTime)
    {
        secondTimer = secondTimer + (deltaTime * timeMultiplier);
        // Debug.Log($"secondTimer value before = {secondTimer}");


        while (secondTimer >= 1f)
        {
            secondTimer = secondTimer - 1.0f; //consume second leaving remainder
            advanceSecond(1); //advance time by a second
            // Debug.Log($"secondTimer value after = {secondTimer}");
            //minuteTimer++;
        }

        //sunrise
        if (currentGameHour == sunriseHour && previousGameHour < sunriseHour)
        {
            onSunrise.Invoke(true);
            Debug.Log("onSunrise Invoked with value of true");

        }

        //sunset
        if (currentGameHour == sunsetHour && previousGameHour < sunsetHour)
        {
            onSunset.Invoke(true);
            Debug.Log("onSunset Invoked with value of true");

        }
        //if a sunshine hour has passed
        if (currentGameHour > previousGameHour &&
        currentGameHour > sunriseHour &&
        currentGameHour < sunsetHour)
        {
            onSunshineHourPassed.Invoke(1);
            Debug.Log("onSunshineHourPassed Invoked with value of 1");

        }

        //check for season change
        if (determineSeason(currentGameMonth) != determineSeason(previousGameMonth))
        {
            //There has to be a better way to do this... switch statement?

            //Summer
            if (determineSeason(currentGameMonth) == seasonOfYear.Summer)
            {
                onChangeToSummer.Invoke(true);
                Debug.Log("onChangeToSummer Invoked");
            }

            //Autumn
            if (determineSeason(currentGameMonth) == seasonOfYear.Autumn)
            {
                onChangeToAutumn.Invoke(true);
                Debug.Log("onChangeToAutumn Invoked");

            }

            //Winter
            if (determineSeason(currentGameMonth) == seasonOfYear.Winter)
            {
                onChangeToWinter.Invoke(true);
                Debug.Log("onChangeToWinter Invoked");

            }

            //Spring
            if (determineSeason(currentGameMonth) == seasonOfYear.Spring)
            {
                onChangeToSpring.Invoke(true);
                Debug.Log("onChangeToSpring Invoked");
            }
        }


        previousGameYear = currentGameYear;
        previousGameMonth = currentGameMonth;
        previousGameDay = currentGameDay;
        previousGameHour = currentGameHour;
        previousGameMinute = currentGameMinute;
        previousGameSecond = currentGameSecond;
        previousGameDayOfWeek = currentGameDayOfWeek;


    }

    void Start()
    {
        //Error with saving/loading.
        //stateSaver.load(gameTimeState);
        //DateTime then = DateTime.Parse(lastSave);
        //proccessTimeDifference(then, DateTime.Now);

        updateGameDayString();
        updateGameTimeString();
    }



    void Update()
    {
        updateGameTime(Time.deltaTime);

    }

    void OnApplicationQuit()
    {
        DateTime now = DateTime.Now;
        lastSave = now.ToString(lastSave);
        // stateSaver.save();
    }

    void advanceYear(int amount)
    {
        previousGameYear = currentGameYear;
        currentGameYear += amount;
    }
    void advanceMonth(int amount)
    {
        previousGameMonth = currentGameMonth;
        currentGameMonth = (currentGameMonth + amount);
        while (currentGameMonth >= 12)
        {
            currentGameMonth -= 12;
            advanceYear(1);
        }
        onMonthChange.Invoke(amount);
        Debug.Log($"onMonthChange Invoked with value of {amount}");
    }
    void advanceHour(int amount)
    {
        previousGameHour = currentGameHour;
        currentGameHour = (currentGameHour + amount);

        while (currentGameHour >= hoursInDay)
        {
            currentGameHour -= hoursInDay;
            advanceDay(1);
        }
        onHourChange.Invoke(amount);
    }
    void advanceDay(int amount)
    {
        previousGameDay = currentGameDay;
        currentGameDay = (currentGameDay + amount);

        previousGameDayOfWeek = currentGameDayOfWeek;
        currentGameDayOfWeek = currentGameDayOfWeek + amount;
        while (currentGameDay > daysInMonth[currentGameMonth])
        {
            currentGameDay -= daysInMonth[currentGameMonth];
            advanceMonth(1);
        }
        while (currentGameDayOfWeek >= 7)
        {
            currentGameDayOfWeek -= 7;
        }
        updateGameDayString(); ////a day has changed, so update the string
        onDayChange.Invoke(amount);
        Debug.Log($"onDayChange Invoked with value of {amount}");

    }
    void advanceMinute(int amount)
    {
        previousGameMinute = currentGameMinute;
        currentGameMinute = currentGameMinute + amount;
        while (currentGameMinute >= 60)
        {
            currentGameMinute -= 60;
            advanceHour(1);
        }
        updateGameTimeString();
    }
    void advanceSecond(int amount)
    {
        previousGameSecond = currentGameSecond;
        currentGameSecond = currentGameSecond + amount;
        while (currentGameSecond >= 60)
        {
            currentGameSecond -= 60;
            advanceMinute(1);
        }
    }

    float calculateDayPercentageCompleted()
    {
        return (currentGameHour + (currentGameMinute / 60f)) / (float)hoursInDay;
    }


    // //proccess the time that has elapsed between playing
    // void proccessTimeDifference(DateTime then, DateTime now)
    // {
    //     TimeSpan elapsedTime = now - then;

    //     //convert to gametime
    //     TimeSpan elapsedGameTime = new TimeSpan((long)(elapsedTime.Ticks * timeMultiplier));

    //     int monthsToAdd = 0;
    //     int elapsedDays = elapsedGameTime.Days;
    //     //apply
    //     while (elapsedDays > daysInMonth[currentGameMonth])
    //     {
    //         elapsedDays = elapsedDays - daysInMonth[currentGameMonth];
    //         monthsToAdd++;
    //     }
    //     int yearsToAdd = 0;
    //     while (monthsToAdd > 12)
    //     {
    //         monthsToAdd = monthsToAdd - 12;
    //         yearsToAdd++;
    //     }

    //     currentGameYear = yearsToAdd;
    //     currentGameMonth = monthsToAdd;
    //     currentGameDay = elapsedGameTime.Days;
    //     currentGameHour = elapsedGameTime.Hours;
    //     currentGameMinute = elapsedGameTime.Minutes;
    //     currentGameSecond = elapsedGameTime.Seconds;

    //     //calculate sunshine hours elapsed
    //     int sunshineHoursElapsed = 0;

    //     DateTime i = then;
    //     i.AddMinutes(-i.Minute);
    //     i.AddSeconds(-i.Second);
    //     i.AddMilliseconds(-i.Millisecond);

    //     while (i < now)
    //     {
    //         TimeSpan current = i.TimeOfDay;

    //         if (current >= TimeSpan.FromHours(sunriseHour) && current < TimeSpan.FromHours(sunsetHour))
    //         {
    //             sunshineHoursElapsed++;
    //         }

    //         i = i.AddHours(1);
    //     }

    //     onSunshineHourPassed.Invoke(sunshineHoursElapsed);

    //     //calculate months changed?
    //     //calculate days changed?
    //     // right now I can't see a reason to impl this
    //     // I will need to come up with a way to impl rainfall while away...
    // }

    //function to return season from given month
    seasonOfYear determineSeason(int monthNum)
    {
        switch (monthNum)
        {
            case 12:
            case 1:
            case 2:
                return seasonOfYear.Summer;
            case 3:
            case 4:
            case 5:
                return seasonOfYear.Autumn;
            case 6:
            case 7:
            case 8:
                return seasonOfYear.Winter;
            case 9:
            case 10:
            case 11:
                return seasonOfYear.Spring;
            default:
                Debug.LogError("Invalid month, returning Summer");
                return seasonOfYear.Summer;
        }
    }

    public int getCurrentGameMonth()
    {
        return currentGameMonth;
    }


    // I am using the description attribute to deal with the Māori names having special characters like "-", which the code disallows.
    enum dayOfWeek
    {
        [Description("Rāhina")] Monday, //(moon)
        [Description("Rātū")] Tuesday, //tuesday (war)
        [Description("Rāapa")] Wednesday, //(group/assembly)
        [Description("Rāpare")] Thursday, //thursday (headband)
        [Description("Rāmere")] Friday, //friday (weapon day)
        [Description("Rāhoroi")] Saturday, //saturday (washing day)
        [Description("Rātapu")] Sunday //sunday (sacred day)
    }
    enum monthOfYear
    {
        [Description("Kohitātea")] January,
        [Description("Huitanguru")] February,
        [Description("Poutūterangi")] March,
        [Description("Paengawhāwhā")] April,
        [Description("Haratua")] May,
        [Description("Pipiri")] June,
        [Description("Hōngoingoi")] July,
        [Description("Here­turi­kōkā")] August,
        [Description("Mahuru")] September,
        [Description("Whiringa-ā-nuku")] October,
        [Description("Whiringa-ā-rangi")] November,
        [Description("Hakihea")] December
    }
    enum seasonOfYear
    {
        [Description("Raumati")] Summer,
        [Description("Ngahuru")] Autumn,
        [Description("Takurua")] Winter,
        [Description("Kōanga")] Spring,
    }

    // Helper Extension Method to read the description

    public static string ToDescriptionString(Enum val)
    {
        FieldInfo field = val.GetType().GetField(val.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute != null ? attribute.Description : val.ToString();
    }



}


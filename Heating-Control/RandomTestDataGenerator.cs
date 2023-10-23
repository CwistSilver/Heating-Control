using Heating_Control.Data;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Heating_Control;
internal static class RandomTestDataGenerator
{
    private const int MaxSupplyTemperature = 90;

    private const int MaxTemp = 20;
    private const int MinTemp = -20;
    private const float HeatF = 3.5f;

    private const int MaxPreferredIndoorTemp = 28;
    private const int MinPreferredIndoorTemp = 19;



    public static List<HeatingControlTrainingData> CreateRandomData(int count = 1_000)
    {
        var list = new List<HeatingControlTrainingData>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(CreateData());
        }

        var text = JsonSerializer.Serialize(list, new JsonSerializerOptions() { WriteIndented = true });

        return list;
    }

    public static HeatingControlTrainingData CreateData()
    {
        var data = new HeatingControlTrainingData();
        data.OutdoorTemperature = RandomOutdoorTemperatureValue();
        data.PreferredIndoorTemperature = RandomPreferredIndoorTemperatureValue();
        data.PredictedOutdoorTemperature = RandomPredictedOutdoorTemperatureValue(data);
        data.SupplyTemperature = RandomSupplyTemperatureValue(data);

        return data;
    }

    private static float RandomPreferredIndoorTemperatureValue()
    {
        float value = Random.Shared.Next(MinPreferredIndoorTemp * 100, MaxPreferredIndoorTemp * 100);
        return value / 100;
    }

    private static float RandomOutdoorTemperatureValue()
    {
        float value = Random.Shared.Next(MinTemp * 100, MaxTemp * 100);
        return value / 100;
    }

    private static float RandomPredictedOutdoorTemperatureValue(HeatingControlTrainingData data)
    {
        var addedValue = Random.Shared.Next(-1_000, 1_000) / 100f; // zahl zwischen -10.00 und +10.00
        var outdoorTemperature = data.OutdoorTemperature + addedValue;
        return Math.Clamp(outdoorTemperature, MinTemp, MaxTemp);
    }



    private static float RandomSupplyTemperatureValue(HeatingControlTrainingData data)
    {
        //data.OutdoorTemperature = 20;
        //data.PredictedOutdoorTemperature = 20;
        //data.PreferredIndoorTemperature = 22;

        var p = Random.Shared.Next(80, 85) / 100f;
        var minTemp = Math.Min(data.PredictedOutdoorTemperature, data.OutdoorTemperature);
        var maxTemp = Math.Max(data.PredictedOutdoorTemperature, data.OutdoorTemperature);
        var temperatureDifference = maxTemp - minTemp;
        var temperatureDifferenceWithToll = maxTemp - temperatureDifference * p;

        var rtSoll = data.PreferredIndoorTemperature;
        var dar = temperatureDifferenceWithToll - data.PreferredIndoorTemperature;
        if (temperatureDifferenceWithToll >= data.PreferredIndoorTemperature)
            return 0;

        var niveau = 0;
        var neigung = 1.5f;
        var vtSoll = rtSoll + niveau - neigung * dar * (1.4347f + 0.021f * dar + 247.9f * Math.Pow(10, -6) * Math.Pow(dar, 2));
        return Math.Clamp((float)vtSoll, temperatureDifferenceWithToll, MaxSupplyTemperature);
        //VTSoll = RTSoll + Niveau -Neigung* DAR * (1.4347 + 0,021 * DAR + 247.9 * 10(^-6) * (DAR^2))
        //VTSoll: Vorlauftemp
        //DAR = Gemischte Ausentemperatur -RTSoll(Deifferen AusenTemp/RaumTemp)
        //RTSoll= RaumSoll


        var valueToHeat = maxTemp - (temperatureDifference * p);



        var outdorIndorDif = Math.Abs(valueToHeat - data.PreferredIndoorTemperature);
        var hF = outdorIndorDif * HeatF;

        var cap = Math.Clamp(hF, valueToHeat, MaxSupplyTemperature);
        return cap;
        // Innen 20 -> Boiler 40
        // Innen 20 -> Boiler 40
        // Innen 20 -> Boiler 40

    }

    //private static float RandomSupplyTemperatureValue(HeatingControlTrainingData data)
    //{

    //    var p = Random.Shared.Next(80, 85) / 100;
    //    var minTemp = Math.Min(data.PredictedOutdoorTemperature, data.OutdoorTemperature);
    //    var maxTemp = Math.Max(data.PredictedOutdoorTemperature, data.OutdoorTemperature);
    //    var temperatureDifference = maxTemp - minTemp;

    //    var valueToHeat = maxTemp - (temperatureDifference * p);

    //    if (valueToHeat >= data.PreferredIndoorTemperature)
    //        return 0;

    //    var outdorIndorDif = Math.Abs(valueToHeat - data.PreferredIndoorTemperature);
    //    var hF = outdorIndorDif * HeatF;

    //    var cap = Math.Clamp(hF, valueToHeat, MaxSupplyTemperature);
    //    return cap;
    //    // Innen 20 -> Boiler 40
    //    // Innen 20 -> Boiler 40
    //    // Innen 20 -> Boiler 40

    //}

}

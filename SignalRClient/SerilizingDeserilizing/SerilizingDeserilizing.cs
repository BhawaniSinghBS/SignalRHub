using Newtonsoft.Json;

namespace SignalRClient.SerilizingDeserilizing
{
    public static class SerilizingDeserilizing
    {
        public static string JSONSerializeOBJ(object objToSerilize)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(objToSerilize);
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return "";
                }
                else
                {
                    return jsonData;
                }

            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static expectedType JSONDeserializeOBJ<expectedType>(string json)
        {
            try
            {
                Type expectedTypeType = typeof(expectedType);
                if (json != null)
                {
                    // Deserialize object to JSON
                    var expectedResultobj = JsonConvert.DeserializeObject<expectedType>(json);
                    //expectedType expectedObject = (expectedType)rawObject;
                    return expectedResultobj ?? (expectedType)new object();
                }
                else
                {
                    return (expectedType)new object();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static string ConvertToBase64String(byte[] byteArray)
        {
            string base64String = Convert.ToBase64String(byteArray);
            return base64String;
        }
        public static byte[] ConvertFromBase64String(string base64DataString)
        {
            byte[] data = Convert.FromBase64String(base64DataString);
            return data;
        }
    }
}

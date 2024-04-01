using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestApp2.Common
{
    public class DateTimeDataConverter
    {
      static  string convertdatetime;
        public static string StringtoDateTime (string Datetime) 
        {
            try
            {
                if (string.IsNullOrEmpty(Datetime))
                    return string.Empty;
               DateTime datetime = DateTime.ParseExact(Datetime, "yyyyMMdd HH:mm:ss.fff", null);
               convertdatetime = datetime.ToString("MM/dd/yyyy hh:mm:ss tt");

            }
            catch (Exception ex)
            {

               
            }


            return convertdatetime;
        }
        static DateTime date;
        public static DateTime StringtoDateTimedata(string Datetime)
        {
            try
            {
                
                   
                date = Convert.ToDateTime(DateTime.ParseExact(Datetime, "yyyyMMdd HH:mm:ss.fff", null).ToString("MM/dd/yyyy hh:mm:ss tt"));

            }
            catch (Exception ex)
            {

                
            }
         

            return date;
        }
    }
}

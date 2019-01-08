using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Libreria.Logger
{
    public class Utiles
    {

        public const string FormatDate = "dd/MM/yyyy";
        public const string FormatDateTime = "dd/MM/yyyy HH:mm";
        public const string FormatDecimal = "N";

        public static bool IsDate(string sdate)
        {
            DateTime dt;
            bool isDate = true;

            try
            {
                dt = DateTime.Parse(sdate);
            }
            catch
            {
                isDate = false;
            }

            return isDate;
        }

        public static bool IsDate(string sdate, string format)
        {
            DateTime dt;
            bool isDate = true;
            try
            {
                dt = DateTime.ParseExact(sdate, format, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                isDate = false;
            }

            return isDate;
        }

        public static bool IsDate(string sdate, string[] formats)
        {
            bool isDate = true;
            foreach (string format in formats)
            {
                if (IsDate(sdate, format) == true)
                {
                    break;
                }
            }
            return isDate;
        }

        public static bool IsDateExcel(string sdate)
        {
            DateTime dt;
            double d = 0;
            bool isDate = true;
            if (IsDate(sdate) == false)
            {
                try
                {
                    d = double.Parse(sdate);
                    dt = DateTime.FromOADate(d);
                }
                catch
                {
                    isDate = false;
                }
            }
            return isDate;
        }


        public static bool IsNumericInteger(object Expression)
        {
            bool isNum;
            int retNum;
            try
            {
                isNum = Int32.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
                return isNum;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNumericDecimal(object Expression)
        {
            bool isNum;
            double retNum;
            try
            {

                isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
                return isNum;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime FromStringToDateTime(string sdate, string format)
        {
            return DateTime.ParseExact(sdate, format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static DateTime FromStringToDateTime(string sdate, string[] formats)
        {
            foreach (string format in formats)
            {
                if (IsDate(sdate, format) == true)
                {
                    return DateTime.ParseExact(sdate, format, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            return DateTime.ParseExact(sdate, formats[0], System.Globalization.CultureInfo.InvariantCulture);
        }

        public static object FromDecimalToDBNull(decimal dec)
        {
            if (dec == (decimal)0)
                return System.DBNull.Value;
            return dec;
        }

        public static decimal FromDBNullToDecimal(object obj)
        {
            if (obj == System.DBNull.Value)
                return (decimal)0;
            return System.Convert.ToDecimal(obj);
        }

        public static decimal FromStringToDecimal(string str)
        {
            if (str.Trim().Equals(string.Empty))
                return (decimal)0;

            return System.Convert.ToDecimal(str);

        }

        public static string FromDecimalToString(decimal dec)
        {
            if (dec == (decimal)0)
                return string.Empty;

            return System.Convert.ToString(dec);

        }

        public static string FromInt32ToString(int entero)
        {
            if (entero == (int)0)
                return string.Empty;

            return System.Convert.ToString(entero);

        }

        public static int FromStringToInt32(string cadena)
        {
            if (cadena == "")
                return 0;

            return System.Convert.ToInt32(cadena);

        }

        public static object FromStringToDBNull(string str)
        {
            if (str == "")
                return System.DBNull.Value;
            return str;
        }

        public static DateTime FromStringDateToDateTime(string stringDate)
        {
            if (stringDate == "")
                return new DateTime();
            return DateTime.Parse(stringDate);
        }

        public static DateTime FromStringDateNewToDateTime(string stringDate)
        {
            if (stringDate == "")
                return new DateTime();

            string sAnio = stringDate.Substring(0, 11);
            sAnio = sAnio.Substring(6, 5);
            sAnio = sAnio.Replace(" ", "");

            string sMes = stringDate.Substring(0, 3);
            sMes = sMes.Replace("Mar", "03");

            string sDia = stringDate.Substring(0, 6);
            sDia = sDia.Substring(4, 2);
            sDia = sDia.Replace(" ", "");

            return DateTime.Parse(sDia + " " + sMes + " " + sAnio);
        }

        public static string FromDBNullToString(object obj)
        {
            if (obj == System.DBNull.Value)
                return string.Empty;
            return System.Convert.ToString(obj);

        }

        public static object FromInt32ToDBNull(int integer)
        {
            if (integer == 0)
                return System.DBNull.Value;
            return integer;
        }

        public static int FromDBNullToInt32(object obj)
        {
            if (obj == System.DBNull.Value)
                return 0;
            return System.Convert.ToInt32(obj);
        }

        public static string FromDateTimeToStringDate(DateTime fecha)
        {
            if (fecha.ToString().StartsWith("01/01/0001"))
                return "";
            return fecha.ToString(FormatDate);
        }

        public static object FromDateTimeToDBNull(DateTime fecha)
        {
            if (fecha == new DateTime() || fecha.ToString().StartsWith("01/01/0001"))
                return System.DBNull.Value;
            return fecha;
        }

        public static DateTime FromDBNullToDateTime(object fecha)
        {
            if (fecha == System.DBNull.Value)
                return new DateTime();
            return System.Convert.ToDateTime(fecha);
        }

        public static bool FromDBNullToBool(object valor)
        {
            if (valor == System.DBNull.Value)
                return false;
            return System.Convert.ToBoolean(valor);
        }
    }
}

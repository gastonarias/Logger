using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

namespace Libreria.Logger
{
    /// <summary>
    /// Clase de log
    /// </summary>
    public class Logger
    {
        private static object syncObject = new Object();

        private string proceso;
        private string ubicacion;
        private string nombre;

        /// <summary>
        /// Logger default de la aplicación
        /// </summary>
        public static Logger Default = new Logger();

        /// <summary>
        /// Inicializa un nuevo logger default usando nombre de la aplicación
        /// </summary>
        private Logger()
        {

            this.nombre = string.Empty;
            this.proceso = LoggerConfig.NombreProceso;
            this.ubicacion = LoggerConfig.Ubicacion;

            ControlPeso();

            try
            {
                // Crea carpeta de logs
                if (!Directory.Exists(this.ubicacion))
                    Directory.CreateDirectory(this.ubicacion);
            }
            catch { }
        }

        /// <summary>
        /// Inicializa un nuevo logger
        /// </summary>
        public Logger(string nombre)
            : this()
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentNullException("El nombre del logger es obligatorio");

            this.nombre = nombre;
        }

        /// <summary>
        /// Devuelve la ruta completa sin el nombre de archivo
        /// </summary>
        public string Ubicacion
        {
            get
            {
                return this.ubicacion;
            }
        }

        /// <summary>
        /// Devuelve la ruta completa del archivo de log que utiliza este logger
        /// </summary>
        public string Archivo
        {
            get
            {
                return ObtenerArchivoLogs();
            }
        }

        /// <summary>
        /// Graba evento conteniendo información técnica que ayude a depurar la aplicación
        /// </summary>
        public void Debug(string mensaje)
        {
            if (LoggerConfig.Debug)
                GrabarMensaje(Severidad.Debug, mensaje);
        }

        /// <summary>
        /// Graba evento conteniendo información técnica que ayude a depurar la aplicación
        /// </summary>
        public void Debug(string formato, params object[] args)
        {
            if (LoggerConfig.Debug)
                GrabarMensaje(Severidad.Debug, ArmarMensaje(formato, args));
        }

        /// <summary>
        /// Graba evento informando situaciones normales de la aplicación
        /// </summary>
        public void Info(string mensaje)
        {
            GrabarMensaje(Severidad.Info, mensaje);
        }

        /// <summary>
        /// Graba evento informando situaciones normales de la aplicación
        /// </summary>
        public void Info(string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Info, ArmarMensaje(formato, args));
        }

        /// <summary>
        /// Graba evento advirtiendo anomalías en un proceso aunque haya finalizado correctamente
        /// (ej. asumir defaults por falta de configuración, o exceso en tiempo de proceso)
        /// </summary>
        public void Warn(string mensaje)
        {
            GrabarMensaje(Severidad.Warn, mensaje);
        }

        /// <summary>
        /// Graba evento advirtiendo anomalías en un proceso aunque haya finalizado correctamente
        /// (ej. asumir defaults por falta de configuración, o exceso en tiempo de proceso)
        /// </summary>
        public void Warn(string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Warn, ArmarMensaje(formato, args));
        }

        /// <summary>
        /// Graba evento advirtiendo anomalías en un proceso aunque haya finalizado correctamente
        /// (ej. asumir defaults por falta de configuración, o exceso en tiempo de proceso)
        /// </summary>
        public void Warn(Exception ex, string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Warn, ArmarMensaje(ex, formato, args));
        }

        /// <summary>
        /// Graba evento cuando un proceso individual no pudo completarse pero la aplicación continúa funcionando
        /// (ej. argumentos en un pedido, o error de acceso a la base de datos)
        /// </summary>
        public void Error(string mensaje)
        {
            GrabarMensaje(Severidad.Error, mensaje);
        }

        /// <summary>
        /// Graba evento cuando un proceso individual no pudo completarse pero la aplicación continúa funcionando
        /// (ej. argumentos en un pedido, o error de acceso a la base de datos)
        /// </summary>
        public void Error(string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Error, ArmarMensaje(formato, args));
        }

        /// <summary>
        /// Graba evento cuando un proceso individual no pudo completarse pero la aplicación continúa funcionando
        /// (ej. argumentos en un pedido, o error de acceso a la base de datos)
        /// </summary>
        public void Error(Exception ex, string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Error, ArmarMensaje(ex, formato, args), ArmarMensajeError(ex, formato, args));
        }

        /// <summary>
        /// Graba evento cuando la aplicación completa no puede continuar realizando procesamiento
        /// (ej. error permanente de conexión a base de datos, o falta de configuración necesaria para iniciar)
        /// </summary>
        public void Fatal(string mensaje)
        {
            GrabarMensaje(Severidad.Fatal, mensaje);
        }

        /// <summary>
        /// Graba evento cuando la aplicación completa no puede continuar realizando procesamiento
        /// (ej. error permanente de conexión a base de datos, o falta de configuración necesaria para iniciar)
        /// </summary>
        public void Fatal(string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Fatal, ArmarMensaje(formato, args));
        }

        /// <summary>
        /// Graba evento cuando la aplicación completa no puede continuar realizando procesamiento
        /// (ej. error permanente de conexión a base de datos, o falta de configuración necesaria para iniciar)
        /// </summary>
        public void Fatal(Exception ex, string formato, params object[] args)
        {
            GrabarMensaje(Severidad.Fatal, ArmarMensaje(ex, formato, args), ArmarMensajeError(ex, formato, args));
        }

        /// <summary>
        /// Grabacion interna de un evento
        /// </summary>
        private void GrabarMensaje(Severidad severidad, string mensaje)
        {
            GrabarMensaje(severidad, mensaje, string.Empty);
        }

        /// <summary>
        /// Grabacion interna de un evento
        /// </summary>
        private void GrabarMensaje(Severidad severidad, string mensaje, string mensajeError)
        {
            try
            {
                // Crea el evento que se grabará a disco
                string evento = FormatearEvento(severidad, mensaje);

                lock (syncObject)
                {
                    string archivo = ObtenerArchivoLogs();
                    GrabarEvento(archivo, evento);

                    // Graba en archivo de error solo si es error o fatal
                    if (severidad == Severidad.Error || severidad == Severidad.Fatal)
                    {
                        archivo = ObtenerArchivoErrores();
                        string eventoError = string.IsNullOrEmpty(mensajeError) ? evento : this.FormatearEvento(severidad, mensajeError);
                        GrabarEvento(archivo, eventoError);
                    }

                    ControlPeso();
                }
            }
            catch { }
        }


        private void ControlPeso()
        {
            FileInfo valorPeso = new FileInfo(this.Archivo);

            if (File.Exists(this.Archivo) && (valorPeso.Length > 30000000))
            {
                valorPeso.MoveTo(string.Format("{0}-{1:00}-{2:00}-{3:00}",
                    this.Archivo, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
            }

        }

        /// <summary>
        /// Realiza append al final del archivo
        /// </summary>
        private void GrabarEvento(string archivo, string evento)
        {
            try
            {
                FileStream stream = new FileStream(archivo, FileMode.Append, FileAccess.Write);
                using (StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding(1252)))
                {
                    writer.Write(evento);
                }
            }
            catch { }
        }

        /// <summary>
        /// Devuelve la linea de log completa para ser grabada
        /// </summary>
        private string FormatearEvento(Severidad severidad, string mensaje)
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1,-5}] {2,-5} - {3}\r\n",
                DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, severidad, mensaje);
        }

        /// <summary>
        /// Arma el mensaje para loggear
        /// </summary>
        private string ArmarMensaje(string formato, params object[] args)
        {
            return string.Format(formato, args);
        }

        /// <summary>
        /// Arma el mensaje para loggear
        /// </summary>
        private string ArmarMensaje(Exception ex, string formato, params object[] args)
        {
            return string.Format("{0}\r\n{1}", ArmarMensaje(formato, args), ex.Message);
        }

        /// <summary>
        /// Arma el mensaje de error para loggear
        /// </summary>
        private string ArmarMensajeError(Exception ex, string formato, params object[] args)
        {
            return string.Format("{0}\r\n{1}", ArmarMensaje(formato, args), ex);
        }

        /// <summary>
        /// Arma el path al archivo de log
        /// </summary>
        private string ObtenerArchivoLogs()
        {
            return ObtenerArchivo("log");
        }

        /// <summary>
        /// Arma el path al archivo de errores
        /// </summary>
        private string ObtenerArchivoErrores()
        {
            return ObtenerArchivo("err");
        }

        /// <summary>
        /// Arma el path a un archivo cualquiera de logs
        /// </summary>
        private string ObtenerArchivo(string extension)
        {
            DateTime hoy = DateTime.Now;

            // Verifica si es un logger default de la aplicacion
            if (string.IsNullOrEmpty(nombre))
                return Path.Combine(ubicacion, string.Format("{0:yyyy-MM-dd}.{1}.{2}", hoy, this.proceso, extension));
            else
                return Path.Combine(ubicacion, string.Format("{0:yyyy-MM-dd}.{1}.{2}.{3}", hoy, this.proceso, this.nombre, extension));
        }
    }

    /// <summary>
    /// Nivel de severidad de log
    /// </summary>
    public enum Severidad
    {
        /// <summary>
        /// Debug solo se usa para mostrar información técnica
        /// que ayude a depurar la aplicación
        /// </summary>
        Debug,
        /// <summary>
        /// Informativo se usa para mostrar situaciones de funcionamiento normal
        /// </summary>
        Info,
        /// <summary>
        /// Adevertencia se usa cuando el proceso pudo completarse aunque hubo ciertas anomalías 
        /// (ej. asumir defaults por falta de configuración, o exceso en tiempo de proceso)
        /// </summary>
        Warn,
        /// <summary>
        /// Error se usa cuando un proceso individual no pudo completarse pero la aplicación continúa funcionando
        /// (ej. argumentos en un pedido, o error de acceso a la base de datos)
        /// </summary>
        Error,
        /// <summary>
        /// Fatal se usa cuando la aplicación no puede continuar procesando requerimientos
        /// (ej. error permanente de conexión a base de datos, o falta de configuración necesaria para iniciar)
        /// </summary>
        Fatal
    }

    /// <summary>
    /// Acceso a la configuracion
    /// </summary>
    internal static class LoggerConfig
    {
        /// <summary>
        /// Devuelve si se graban eventos de debug
        /// </summary>
        internal static bool Debug
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["Logger.Debug"];
                return valor != null ? valor.StartsWith("s", StringComparison.CurrentCultureIgnoreCase) : false;
            }
        }

        /// <summary>
        /// Devuelve el nombre del proceso.
        /// Por defecto se obtiene el nombre reflection (solo en aplicaciones desktop)
        /// y se puede modificar con el parámetro "Logger.Proceso" del archivo de configuración
        /// </summary>
        internal static string NombreProceso
        {
            get
            {
                string valor = ConfigurationManager.AppSettings["Aplicacion.Nombre"];
                if (valor != null)
                    return valor;

                valor = ConfigurationManager.AppSettings["Logger.Proceso"];
                if (valor != null)
                    return valor;

                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                    valor = entryAssembly.GetName().Name;
                else
                    valor = string.Empty;

                return valor;
            }
        }

        /// <summary>
        /// Devuelve la carpeta donde se almacenan los logs
        /// Por defecto se utiliza "D:\Logs"
        /// y se puede modificar con el parámetro "Logger.Ubicacion" del archivo de configuración
        /// </summary>
        internal static string Ubicacion
        {
            get
            {
                string valor = ConfigurationManager.AppSettings["Logger.Ubicacion"];

                if (string.IsNullOrEmpty(valor))
                {
                    //valor = Directory.GetCurrentDirectory();  /*@"D:\Logs";*/
                    valor = @"D:\Logs";
                }

                return valor;
            }
        }
    }
}
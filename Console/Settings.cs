using System;
using System.Configuration;

namespace Console
{
    public interface ISettings
    {
        string GetUsage();
        string GetNUnitPath();
        string GetOpenCoverPath();
        string GetReportGeneratorPath();
    }

    public class Settings : ISettings
    {
        public string GetUsage()
        {
            return Properties.Resources.Usage;
        }

        public string GetNUnitPath()
        {
            var path = ConfigurationManager.AppSettings["NUnitPath"];
            if (path == null)
                throw new SettingsException("Could not find 'NUnitPath' in appsettings");

            return path;
        }

        public string GetOpenCoverPath()
        {
            var path = ConfigurationManager.AppSettings["OpenCoverPath"];
            if (path == null)
                throw new SettingsException("Could not find 'OpenCoverPath' in appsettings");

            return path;
        }

        public string GetReportGeneratorPath()
        {
            var path = ConfigurationManager.AppSettings["ReportGeneratorPath"];
            if (path == null)
                throw new SettingsException("Could not find 'ReportGeneratorPath' in appsettings");

            return path;
        }
    }

    public class SettingsException : Exception
    {
        public SettingsException(string message) : base(message)
        {            
        }
    }
}

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ce77f246-871d-4e8c-8e5f-b44ebdbf2541")]

// Location of Log4Net configuration file
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4Net.config")]

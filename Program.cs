using DirectShowLib;

namespace CamParam
{
    class Program
    {
        private static void Main(string[] args)
        {
            // Get DS device.
            DsDevice[] videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // If there are no arguments, list devices and exit;
            if (args.Length == 0)
            {
                for (int i = 0; i < videoInputDevices.Length; i++)
                {
                    DsDevice device = videoInputDevices[i];
                    System.Console.WriteLine(i + " " + device.Name);
                }
                return;
            }

            // Otherwise convert arguments into a table of commands.
            System.Collections.Hashtable commandTable = new System.Collections.Hashtable();

            for (int i = 0; i < args.Length; i += 2)
            {
                // Make sure there is a value after the property.
                if (i + 1 == args.Length)
                {
                    System.Console.Error.WriteLine("Missing value for property: " + args[i]);
                    break;
                }

                // Get property and value.
                string propertyName = args[i];
                string propertyValueString = args[i + 1];
                int propertyValue;

                if (!System.Int32.TryParse(propertyValueString, out propertyValue))
                {
                    System.Console.Error.WriteLine("Property value must be an integer: " + propertyValueString);
                    continue;
                }

                // Add command.
                commandTable[propertyName] = propertyValue;
            }

            // Get device index.
            int deviceIndex = 0;
            if (commandTable.ContainsKey("device"))
            {
                deviceIndex = (int)commandTable["device"];
                commandTable.Remove("device");
            }

            // Get device.
            DsDevice videoInputDevice;
            if (deviceIndex < videoInputDevices.Length)
            {
                videoInputDevice = videoInputDevices[deviceIndex];
            }
            else
            {
                System.Console.Error.WriteLine("Invalid device index: " + deviceIndex);
                return;
            }

            // Set up filter graph.
            IFilterGraph2 filterGraph = new FilterGraph() as IFilterGraph2;
            IBaseFilter baseFilter;
            filterGraph.AddSourceFilterForMoniker(videoInputDevice.Mon, null, videoInputDevice.Name, out baseFilter);

            // Get control objects.
            IAMCameraControl cameraControl = baseFilter as IAMCameraControl;
            IAMVideoProcAmp videoProcAmp = baseFilter as IAMVideoProcAmp;

            if (videoProcAmp == null || cameraControl == null)
            {
                System.Console.Error.WriteLine("Error accessing device: " + videoInputDevice.Name);
                return;
            }

            // Print current values if no commands.
            if (commandTable.Count == 0)
            {
                // Get CameraControlProperty values.
                foreach (CameraControlProperty cameraControlProperty in
                    System.Enum.GetValues(typeof(CameraControlProperty)))
                {
                    int value;
                    CameraControlFlags flags;
                    cameraControl.Get(cameraControlProperty, out value, out flags);
                    System.Console.WriteLine(cameraControlProperty.ToString() + " " + value);
                }

                // Get VideoProcAmpProperty values.
                foreach (VideoProcAmpProperty videoProcAmpProperty in
                    System.Enum.GetValues(typeof(VideoProcAmpProperty)))
                {
                    int value;
                    VideoProcAmpFlags flags;
                    videoProcAmp.Get(videoProcAmpProperty, out value, out flags);
                    System.Console.WriteLine(videoProcAmpProperty.ToString() + " " + value);
                }
            }

            // Loop through remaining commands.
            foreach (System.Collections.DictionaryEntry command in commandTable)
            {
                string propertyName = (string)command.Key;
                int propertyValue = (int)command.Value;

                // Try property as a CameraControlProperty.
                CameraControlProperty cameraControlProperty;
                if (System.Enum.TryParse<CameraControlProperty>(propertyName, true, out cameraControlProperty))
                {
                    System.Console.WriteLine(cameraControlProperty + " " + propertyValue);
                    int result = cameraControl.Set(cameraControlProperty, propertyValue, CameraControlFlags.Manual);
                    if (result != 0) System.Console.Error.WriteLine("Could not set property.");
                    continue;
                }

                // Try property as a VideoProcAmpProperty.
                VideoProcAmpProperty videoProcAmpProperty;
                if (System.Enum.TryParse<VideoProcAmpProperty>(propertyName, true, out videoProcAmpProperty))
                {
                    System.Console.WriteLine(videoProcAmpProperty + " " + propertyValue);
                    int result = videoProcAmp.Set(videoProcAmpProperty, propertyValue, VideoProcAmpFlags.Manual);
                    if (result != 0) System.Console.Error.WriteLine("Could not set property.");
                    continue;
                }

                System.Console.Error.WriteLine("Unrecognized property: " + propertyName);
            }
        }
    }
}
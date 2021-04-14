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
                System.Console.WriteLine("Available devices:");
                for (int i = 0; i < videoInputDevices.Length; i++)
                {
                    DsDevice device = videoInputDevices[i];
                    System.Console.WriteLine(i + " " + device.Name);
                }
                return;
            }

            // Otherwise convert arguments into a table of commands.
            System.Collections.Hashtable commandTable = new System.Collections.Hashtable();

            foreach (string arg in args)
            {
                if (arg.Contains("="))
                {
                    string[] keyValuePair = arg.Split("=");
                    string propertyName = keyValuePair[0];
                    int propertyValue;

                    if (!System.Int32.TryParse(keyValuePair[1], out propertyValue))
                    {
                        System.Console.Error.WriteLine("Property value must be an integer: " + arg);
                        continue;
                    }

                    commandTable[propertyName.ToLower()] = propertyValue;
                }
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
                System.Console.WriteLine("Configuring Device: " + videoInputDevice.Name);
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
                    System.Console.WriteLine(cameraControlProperty.ToString() + "=" + value);
                }

                // Get VideoProcAmpProperty values.
                foreach (VideoProcAmpProperty videoProcAmpProperty in
                    System.Enum.GetValues(typeof(VideoProcAmpProperty)))
                {
                    int value;
                    VideoProcAmpFlags flags;
                    videoProcAmp.Get(videoProcAmpProperty, out value, out flags);
                    System.Console.WriteLine(videoProcAmpProperty.ToString() + "=" + value);
                }
            }

            // Loop through remaining commands.
            foreach (System.Collections.DictionaryEntry command in commandTable)
            {
                string propertyName = (string)command.Key;
                int propertyValue = (int)command.Value;

                System.Console.WriteLine("Setting " + propertyName + " to " + propertyValue);

                // Try property as a CameraControlProperty.
                CameraControlProperty cameraControlProperty;
                if (System.Enum.TryParse<CameraControlProperty>(propertyName, true, out cameraControlProperty))
                {
                    int result = cameraControl.Set(cameraControlProperty, propertyValue, CameraControlFlags.Manual);
                    if (result != 0) System.Console.Error.WriteLine("Could not set property.");
                    continue;
                }

                // Try property as a VideoProcAmpProperty.
                VideoProcAmpProperty videoProcAmpProperty;
                if (System.Enum.TryParse<VideoProcAmpProperty>(propertyName, true, out videoProcAmpProperty))
                {
                    int result = videoProcAmp.Set(videoProcAmpProperty, propertyValue, VideoProcAmpFlags.Manual);
                    if (result != 0) System.Console.Error.WriteLine("Could not set property.");
                    continue;
                }

                System.Console.Error.WriteLine("Unrecognized property: " + propertyName);
            }
        }
    }
}
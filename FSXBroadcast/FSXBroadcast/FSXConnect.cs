using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FSXBroadcast
{
    public delegate void FSXConnectDelegate();
    public delegate void FSXMessageDelegate(string message);

    class FSXConnect
    {
        SimConnect simconnect = null;
        uint seq_id = 0; 

        public const int WM_USER_SIMCONNECT = 0x0402;

        public event FSXConnectDelegate FSXConnectionChanged;
        public event FSXMessageDelegate FSXDataReceived;

        enum DEFINITIONS
        {
            PositionData_Definition,
        }

        enum DATA_REQUESTS
        {
            REQUEST_1S,
        };

        enum EVENTS
        {
            EVENT_1S,
        }

        // this is how you declare a data structure so that
        // simconnect knows how to fill it/read it.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct PositionData
        {
            // this is how you declare a fixed size string
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String title;
            public double latitude;
            public double longitude;
            public double altitude;
            public double airspeed;
            public double groundspeed;
            public double heading;
            public double trueheading;
        };

        public FSXConnect()
        {
        }

        public void connect(IntPtr hWnd)
        {
            if (simconnect != null)
                return;

            try
            {                
                simconnect = new SimConnect("FSX Broadcast", hWnd, WM_USER_SIMCONNECT, null, 0);
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

                // listen to exceptions
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                simconnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
                // define a data structure
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Plane Altitude", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Airspeed Indicated", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Ground Velocity", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Plane Heading Degrees Magnetic", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.PositionData_Definition, "Plane Heading Degrees True", "radians", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                // IMPORTANT: register it with the simconnect managed wrapper marshaller
                // if you skip this step, you will only receive a uint in the .dwData field.
                simconnect.RegisterDataDefineStruct<PositionData>(DEFINITIONS.PositionData_Definition);

                // catch a simobject data request
                // simconnect.SubscribeToSystemEvent(EVENTS.EVENT_1S, "1sec");

                simconnect.RequestDataOnSimObject(DATA_REQUESTS.REQUEST_1S,
                                    DEFINITIONS.PositionData_Definition, SimConnect.SIMCONNECT_OBJECT_ID_USER,
                                    SIMCONNECT_PERIOD.SECOND, SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT, 0, 1, 0);
            }
            catch (COMException ex)
            {
                Console.WriteLine("Connection to FSX failed: {0}", ex.ToString());
            }
            FSXConnectionChanged();
        }

        public void disconnect()
        {
            if (simconnect != null)
            {
                simconnect.UnsubscribeFromSystemEvent(EVENTS.EVENT_1S);
                simconnect.Dispose();
                simconnect = null;
            }
            FSXConnectionChanged();
        }

        public bool isConnected()
        {
            return simconnect != null;
        }

        public void ReceiveMessage()
        {
            simconnect.ReceiveMessage();
        }

        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            FSXConnectionChanged();
        }

        // The case where the user closes FSX
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            disconnect();
        }

        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.WriteLine("Exception received: " + data.dwException);
        }

        void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            PositionData s1 = (PositionData)data.dwData[0];
            double heading = s1.heading * 180.0 / Math.PI;
            double trueheading = s1.trueheading * 180.0 / Math.PI;
            string message = string.Format("{7}|{0:f10}|{1:f10}|{2:f0}|{3:f0}|{4:f0}|{5:f0}|{6:f0}", s1.latitude, s1.longitude, s1.altitude, s1.airspeed, s1.groundspeed, heading, trueheading, seq_id++);
            Console.WriteLine(message);
            // TODO: add timestamp
            if (FSXDataReceived != null)
                FSXDataReceived(message);
        }
    }
}

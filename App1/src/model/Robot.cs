using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.Bluetooth;

using Java.Util;

namespace App1.src.model
{
    class Robot
    {
        public enum BodyCommands
        {
            FORWARD,
            BACKWARD,
            TURN_LEFT,
            TURN_RIGHT,
            STOP,
        }

        public enum HeadCommands
        {
            LEFT,
            RIGHT,
            STOP
        }

        // Conexão com o Robô
        private BluetoothSocket btSocket = null;

        // Id Unico de comunicação
        public static readonly string DEFAULT_UUID = "00001101-0000-1000-8000-00805F9B34FB";

        // MAC Address do Raspberry
        public static readonly string DEFAULT_ADDRESS = "B8:27:EB:BF:C4:56";

        public BluetoothSocket BtSocket { get => btSocket; }

        public Robot(BluetoothSocket btSocket)
        {
            this.btSocket = btSocket;
        }
    }
}
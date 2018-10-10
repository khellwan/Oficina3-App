using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;

namespace App1
{
    class Robot
    {
        // Get the default adapter
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;

        // Id Unico de comunicação
        private static readonly UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        // MAC Address do Raspberry
        private static readonly string address = "B8:27:EB:BF:C4:56";

        //Streams de leitura I/O
        private Stream outStream = null;
        private Stream inStream = null;

        public BluetoothSocket BtSocket { get => btSocket; }

        public Robot()
        {
            // Atribuímos o sensor Bluetooth com que vamos trabalhar
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // Verificamos que está habilitado
            if (!mBluetoothAdapter.Enable())
            {
                throw new System.ApplicationException("Bluetooth desativado!");
            }
            // Verificamos que não é nulo o sensor
            if (mBluetoothAdapter == null)
            {
                throw new System.ApplicationException("Bluetooth não existe ou está ocupado!");
            }
        }

        public void StartConnection(Action DataListener)
        {
            //Iniciamos a conexão com o Raspberry
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            System.Console.WriteLine("Conectando... " + device);

            //Indicamos o adaptador que não seja visível
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                //Inicamos o socket de comunicação com o Raspberry
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);

                //Conectamos o socket
                btSocket.Connect();
                System.Console.WriteLine("Conectado com Sucesso!");
            }
            catch (System.Exception e)
            {
                //em caso de gerarmos erro fechamos o socket
                Console.WriteLine(e.Message);
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception)
                {
                    System.Console.WriteLine("Não foi possível conectar");
                }
                System.Console.WriteLine("Socket Criado");
            }
            // Uma vez conectados ao bluetooth mandamos chamar o metodo que gerará a thread que receberá as mensagens do Raspberry
            DataListener();

            //Envio da string
            writeData(new Java.Lang.String("Ola mundo"));
        }

        public void CloseConnection()
        {
            if (btSocket.IsConnected)
            {
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void SendCommand(string s)
        {
            writeData(new Java.Lang.String(s));
        }

        private void writeData(Java.Lang.String data)
        {
            //Extrair a stream de saida
            try
            {
                outStream = BtSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Erro ao enviar " + e.Message);
            }

            //criar a string que enviaremos
            Java.Lang.String message = data;

            //converter em bytes
            byte[] msgBuffer = message.GetBytes();

            try
            {
                //Escrever no buffer o que acabamos de gerar
                outStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Erro ao enviar " + e.Message);
            }
        }
    }
}
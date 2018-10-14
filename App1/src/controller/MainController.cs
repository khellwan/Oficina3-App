using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Java.Util;

using App1.src.model;

namespace App1.src.controller
{
    class MainController
    {
        private static readonly MainController instance = new MainController();

        private BluetoothAdapter mBluetoothAdapter;

        private Robot robot;

        private bool initialized;

        private MainController()
        {
            initialized = false;
        }

        public static MainController GetInstance()
        {
            return instance;
        }

        public void Initialize()
        {
            // Indicar que estamos tentando inicializar
            initialized = false;

            // Ligar o adaptador Bluetooth do celular para nossa aplicação
            TurnOnBluetooth();

            // Criar um socket de conexão Bluetooth com o Robô
            BluetoothSocket btSocket = CreateConnection(Robot.DEFAULT_UUID, Robot.DEFAULT_ADDRESS);

            // Criar uma representação do Robô
            robot = new Robot(btSocket);

            // Indicar que o processo de inicialização foi concluído com sucesso
            initialized = true;
        }

        public void Stop()
        {
            CloseConnection(robot.BtSocket);
            robot = null;
            initialized = false;
        }

        public void SetRobotMovement(float speed, float rotate)
        {
            if (initialized)
            {
                string cmd = "body " + speed.ToString("0.000") + " " + rotate.ToString("0.000");
                WriteData(robot.BtSocket, cmd);
            }
        }

        public void SetRobotHeadRotation(float speed)
        {
            if (initialized)
            {
                string cmd = "head " + speed.ToString("0.000");
                WriteData(robot.BtSocket, cmd);
            }
        }

        private void TurnOnBluetooth()
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
                throw new System.ApplicationException("Bluetooth indisponível ou está ocupado!");
            }
        }

        private BluetoothSocket CreateConnection(UUID uuid, String address)
        {
            BluetoothSocket btSocket = null;

            // Iniciamos a conexão com o dispositivo solicitado
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);

            System.Console.WriteLine("Conectando... " + device);

            // Indicamos ao adaptador que não seja visível
            mBluetoothAdapter.CancelDiscovery();

            try
            {
                // Inicamos o socket de comunicação com o Raspberry
                btSocket = device.CreateRfcommSocketToServiceRecord(uuid);

                // Conectamos o socket
                btSocket.Connect();

                System.Console.WriteLine("Conectado com Sucesso!");
            }
            catch (System.Exception e)
            {
                // Em caso de erro, fechamos o socket
                try
                {
                    btSocket.Close();
                }
                catch (System.Exception ex) 
                {
                    throw ex;
                }
                throw e;
            }
            return btSocket;
        }
        
        private void CloseConnection(BluetoothSocket btSocket)
        {
            // Checar se é um socket válido e conectado
            if (btSocket != null && btSocket.IsConnected)
            {
                try
                {
                    // Tentar fechar a conexão
                    btSocket.Close();

                    mBluetoothAdapter = null;
                }
                catch (System.Exception e)
                {
                    throw new System.ApplicationException("Erro ao fechar o socket Bluetooth.", e);
                }
            }
        }

        private void WriteData(BluetoothSocket btSocket, string data)
        {
            // Converter a string em bytes
            byte[] msgBuffer = (new Java.Lang.String(data)).GetBytes();

            // Acessar o buffer de saída do socket e enviar a string convertida em bytes
            try
            {
                btSocket.OutputStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException("Erro ao enviar os dados por Bluetooth.", e);
            }
        }
    }
}
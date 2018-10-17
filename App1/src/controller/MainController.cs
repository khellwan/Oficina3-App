using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    sealed class MainController
    {
        private static MainController instance = null;
        private static readonly object padlock = new object();

        private BluetoothAdapter mBluetoothAdapter;

        private Robot robot;

        private bool initialized;

        private Mutex connectionMutex;

        private Mutex bodyCommandMutex;

        private int bodyCommandStack;

        private Mutex headCommandMutex;

        private int headCommandStack;

        MainController()
        {
            initialized = false;
            connectionMutex = new Mutex();
            bodyCommandMutex = new Mutex();
            headCommandMutex = new Mutex();
            bodyCommandStack = 0;
            headCommandStack = 0;
        }

        public static MainController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new MainController();
                        }
                    }
                }
                return instance;
            }
        }

        public void Initialize()
        {
            // Indicar que estamos tentando inicializar
            initialized = false;

            // Ligar o adaptador Bluetooth do celular para nossa aplicação
            TurnOnBluetooth();

            // Criar um socket de conexão Bluetooth com o Robô
            BluetoothSocket btSocket = CreateConnection(Robot.DEFAULT_UUID, Robot.DEFAULT_ADDRESS);
            if (btSocket == null)
            {
                throw new System.ApplicationException("Robô não encontrado.");
            }

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

        public void SetRobotMovement(Robot.BodyCommands cmd)
        {
            bodyCommandMutex.WaitOne();
            switch (cmd)
            {
                case Robot.BodyCommands.FORWARD:
                    bodyCommandStack++;
                    SetRobotMovement(0.75f, 0.0f);
                    break;
                case Robot.BodyCommands.BACKWARD:
                    bodyCommandStack++;
                    SetRobotMovement(-0.75f, 0.0f);
                    break;
                case Robot.BodyCommands.TURN_LEFT:
                    bodyCommandStack++;
                    SetRobotMovement(0.0f, 0.75f);
                    break;
                case Robot.BodyCommands.TURN_RIGHT:
                    bodyCommandStack++;
                    SetRobotMovement(0.0f, -0.75f);
                    break;
                case Robot.BodyCommands.STOP:
                default:
                    bodyCommandStack--;
                    if (bodyCommandStack <= 0)
                    {
                        bodyCommandStack = 0;
                        SetRobotMovement(0.0f, 0.0f);
                    }
                    break;
            }
            bodyCommandMutex.ReleaseMutex();
        }

        public void SetRobotHeadRotation(float speed)
        {
            if (initialized)
            {
                string cmd = "head " + speed.ToString("0.000");
                WriteData(robot.BtSocket, cmd);
            }
        }

        public void SetRobotHeadRotation(Robot.HeadCommands cmd)
        {
            headCommandMutex.WaitOne();
            switch (cmd)
            {
                case Robot.HeadCommands.LEFT:
                    headCommandStack++;
                    SetRobotHeadRotation(0.75f);
                    break;
                case Robot.HeadCommands.RIGHT:
                    headCommandStack++;
                    SetRobotHeadRotation(-0.75f);
                    break;
                case Robot.HeadCommands.STOP:
                default:
                    headCommandStack--;
                    if (headCommandStack <= 0)
                    {
                        headCommandStack = 0;
                        SetRobotHeadRotation(0.0f);
                    }
                    break;
            }
            headCommandMutex.ReleaseMutex();
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

        private BluetoothSocket CreateConnection(String uuid, String address)
        {
            // Trava de acesso a essa função
            if (!connectionMutex.WaitOne(100))
            {
                return null;
            }

            BluetoothDevice pairedBTDevice = null;
            BluetoothSocket btSocket = null;

            // Preparando ...
            try
            {
                // Iniciamos a conexão com o dispositivo solicitado
                pairedBTDevice = mBluetoothAdapter.GetRemoteDevice(address);

                // Indicamos ao adaptador que não seja visível
                mBluetoothAdapter.CancelDiscovery();
            }
            catch (Exception e)
            {
                connectionMutex.ReleaseMutex();
                throw new System.ApplicationException("Erro ao encontrar o Robô.", e);
            }
            
            // Conectando ...
            try
            {
                // Inicamos o socket de comunicação com o Raspberry
                btSocket = pairedBTDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(uuid));

                // Tentar conectar ao socket em 1 segundo
                btSocket.ConnectAsync();
                Thread.Sleep(1000);

                // Checar o resultado
                if (btSocket.IsConnected)
                {
                    connectionMutex.ReleaseMutex();
                    return btSocket;
                }
                else
                {
                    btSocket.Close();
                }
            }
            catch (Exception e)
            {
                // Em caso de erro, fechamos o socket
                try
                {
                    btSocket.Close();
                }
                catch (Exception ex)
                {
                    // Ignore
                }
                connectionMutex.ReleaseMutex();
                throw e;
            }

            connectionMutex.ReleaseMutex();
            return null;
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
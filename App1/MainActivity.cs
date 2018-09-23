using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.IO;
using Java.Util;
using Android.Bluetooth;
using System.Threading.Tasks;
using Android.Content;

namespace App1
{
    [Activity(Label = "App1", MainLauncher = true, Icon = "@drawable/icon")]

    public class MainActivity : Activity
    {

        // Get the default adapter
        private BluetoothAdapter mBluetoothAdapter = null;
        private BluetoothSocket btSocket = null;

        //Id Unico de comunicação
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

        //MAC Address do Raspberry
        private static string address = "B8:27:EB:BF:C4:56";

        //Streams de leitura I/O
        private Stream outStream = null;
        private Stream inStream = null;

        //String a enviar
        private Java.Lang.String dataToSend;

        // Visual
        ToggleButton conectar;
        TextView Resultado;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            conectar = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            Resultado = FindViewById<TextView>(Resource.Id.textView1);

            conectar.CheckedChange += tgConnect_HandleCheckedChange;


            CheckBt();
        }
        private void CheckBt()
        {
            //Atribuímos o sensor Bluetooth com i que vamos trabalhar
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            //Verificamos que está habilitado
            if (!mBluetoothAdapter.Enable())
            {
                Toast.MakeText(this, "Bluetooth desativado",
                    ToastLength.Short).Show();
            }
            //verificamos que não é nulo o sensor
            if (mBluetoothAdapter == null)
            {
                Toast.MakeText(this,
                    "Bluetooth não existe ou está ocupado", ToastLength.Short)
                    .Show();
            }
        }
        //Evento de troca de estado do toggle button
        void tgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                //se é ativado o toggle button se incia o método de conexão
                Connect();
            }
            else
            {
                //em caso de desativar o toggle button se desconecta do Raspberry
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
        }

        //Evento de conexão ao Bluetooth
        public void Connect()
        {
            //Iniciamos a conexão com o Raspberry
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            System.Console.WriteLine("Conectando..." + device);

            //Indicamos o adaptador que não seja visível
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                //Inicamos o socket de comunicação com o Raspberry
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                //Conectamos o socket
                btSocket.Connect();
                System.Console.WriteLine("Conectado com Sucesso");
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
            //Uma vez conectados ao bluetooth mandamos chamar o metodo que gerará o fio
            //que reciberá os dados do Raspberry
            beginListenForData();
            
            //Envio da string
            dataToSend = new Java.Lang.String("Ola mundo");
            writeData(dataToSend);
        }

        public void beginListenForData()
        {
            //Extraímos a stream de entrada
            try
            {
                inStream = btSocket.InputStream;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Creamos un fio que estará correndo em background o qual verificará se há algum dado
            //por parte do Raspberry
            Task.Factory.StartNew(() => {
                //declaramos o buffer de onde guardaremos a leitura
                byte[] buffer = new byte[1024];
                //Declaramos o número de bytes recebidos
                int bytes;
                while (true)
                {
                    try
                    {
                        //Lemos o buffer de entrada e atribuímos a quantidade de bytes que entram
                        bytes = inStream.Read(buffer, 0, buffer.Length);
                        //Verificamos que os bytes contenham informação
                        if (bytes > 0)
                        {
                            //Interface principal
                            RunOnUiThread(() => {
                                //Convertemos o valor da informação em string
                                string valor = System.Text.Encoding.ASCII.GetString(buffer);
                                //Adicionamos ao nosso label a informação que chegou
                                Resultado.Text = Resultado.Text + "\n" + valor;
                            });
                        }
                    }
                    catch (Java.IO.IOException)
                    {
                        //Em caso de erro limpamos o label e cortamos o fio de comunicação
                        RunOnUiThread(() => {
                            Resultado.Text = string.Empty;
                        });
                        break;
                    }
                }
            });
        }
        //Método de envio de dados Bluetooth
        private void writeData(Java.Lang.String data)
        {
            //Extrair a stream de saida
            try
            {
                outStream = btSocket.OutputStream;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Erro ao enviar" + e.Message);
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
                System.Console.WriteLine("Erro ao enviar" + e.Message);
            }
        }






    }



}


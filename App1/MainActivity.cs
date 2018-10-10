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

        private Robot robot;


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

            robot = new Robot();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            conectar = FindViewById<ToggleButton>(Resource.Id.toggleButton1);
            Resultado = FindViewById<TextView>(Resource.Id.textView1);

            conectar.CheckedChange += tgConnect_HandleCheckedChange;
        }

        //Evento de troca de estado do toggle button
        void tgConnect_HandleCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                //se é ativado o toggle button se incia o método de conexão
                robot.StartConnection(delegate() {
                    beginListenForData();
                });
            }
            else
            {
                //em caso de desativar o toggle button se desconecta do Raspberry
                robot.CloseConnection();
            }
        }

        public void beginListenForData()
        {
            //Extraímos a stream de entrada
            try
            {
                inStream = robot.BtSocket.InputStream;
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
    }



}


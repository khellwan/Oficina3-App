using Android.App;
using Android.Widget;
using Android.OS;

using App1.src.controller;
using App1.src.model;

namespace App1
{
    [Activity(Label = "Baby-8", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]

    public class MainActivity : Activity
    {
        // Criar uma representação do Robô
        private Robot robot;
        private Button buttonRobotUp;
        private Button buttonRobotDown;
        private Button buttonRobotLeft;
        private Button buttonRobotRight;
        private Button buttonHeadLeft;
        private Button buttonHeadRight;
        private Button buttonConnect;
        private AbsoluteLayout mainScreen;
        private RelativeLayout overlayScreen;
        private GridLayout contentScreen;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mainScreen = FindViewById<AbsoluteLayout>(Resource.Id.mainScreen);

            // Pegar a tela com os botões de comando do Robô
            contentScreen = FindViewById<GridLayout>(Resource.Id.contentScreen);

            // Pegar o Overlay que bloqueia o aplicativo até ele se conectar ao Robô
            overlayScreen = FindViewById<RelativeLayout>(Resource.Id.overlayScreen);
            overlayScreen.Visibility = Android.Views.ViewStates.Visible;

            // Mapear os botões de controle do Corpo do Robô
            buttonRobotUp = FindViewById<Button>(Resource.Id.btnUp);
            buttonRobotDown = FindViewById<Button>(Resource.Id.btnDown);
            buttonRobotLeft = FindViewById<Button>(Resource.Id.btnLeft);
            buttonRobotRight = FindViewById<Button>(Resource.Id.btnRight);
            // Adicionar eventos aos botões
            buttonRobotUp.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotMovement(0.75f, 0.0f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotMovement(0.0f, 0.0f);
                        break;
                }
            };
            buttonRobotDown.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotMovement(-0.75f, 0.0f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotMovement(0.0f, 0.0f);
                        break;
                }
            };
            buttonRobotLeft.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotMovement(0.0f, 0.75f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotMovement(0.0f, 0.0f);
                        break;
                }
            };
            buttonRobotRight.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotMovement(0.0f, -0.75f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotMovement(0.0f, 0.0f);
                        break;
                }
            };

            // Mapear os botões de controle da Cabeça do Robô
            buttonHeadLeft = FindViewById<Button>(Resource.Id.btnHeadLeft);
            buttonHeadRight = FindViewById<Button>(Resource.Id.btnHeadRight);
            // Adicionar eventos aos botões
            buttonHeadLeft.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotHeadRotation(-0.75f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotHeadRotation(0.0f);
                        break;
                }
            };
            buttonHeadRight.Touch += (o, e) =>
            {
                switch (e.Event.Action)
                {
                    case Android.Views.MotionEventActions.Down:
                        MainController.GetInstance().SetRobotHeadRotation(0.75f);
                        break;
                    case Android.Views.MotionEventActions.Up:
                    case Android.Views.MotionEventActions.Cancel:
                        MainController.GetInstance().SetRobotHeadRotation(0.0f);
                        break;
                }
            };

            // Mapear o botão de conectar-se ao Robô por meio do Bluetooth
            buttonConnect = FindViewById<Button>(Resource.Id.btnConnect);
            // Adicionar evento ao botão
            buttonConnect.Click += (o, e) =>
            {
                try
                {
                    //MainController.GetInstance().Initialize();

                    overlayScreen.Visibility = Android.Views.ViewStates.Gone;
                    mainScreen.RemoveView(overlayScreen);
                    Toast.MakeText(this, "Conectado com sucesso!", ToastLength.Short).Show();
                }
                catch (System.Exception ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MainController.GetInstance().Stop();
        }
    }



}


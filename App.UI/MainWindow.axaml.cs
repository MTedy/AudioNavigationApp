using Avalonia.Controls;
using Avalonia.Input;
using App.Core;
using System;
using Avalonia.Threading;
using System.IO;
using System.Text.Json;
using System.Data;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using App.Audio;


namespace App.UI;

public partial class MainWindow : Window
{

    private Map _currentMap;
    private AudioEngine _audioEngine;

    private Player _player;
    private const double Speed = 2.0;//Prędkość chodzenia
    private DispatcherTimer _gameLoop;

    //Stany klawiszy
    private bool _isWPressed, _isSPressed, _isAPressed, _isDPressed;//poruszanie się WSAD
    private bool _isQPressed, _isEPressed;//Obrót QE

    //Wczytywanie mapy
    private void LoadMap(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            _currentMap = JsonSerializer.Deserialize<Map>(json);

            _player.X = _currentMap.PlayerStartX;
            _player.Y = _currentMap.PlayerStartY;

            //Rysowanie punkltów dźwiękowych
            foreach (var landmark in _currentMap.Landmarks)
            {
                Ellipse dot = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = Brushes.LimeGreen
                };

                //Ustawianie kropki na odpowiednich współrzędnych
                Canvas.SetLeft(dot, landmark.X - 8);
                Canvas.SetTop(dot, landmark.Y - 8);

                MainCanvas.Children.Add(dot);
                _audioEngine.AddLoopingSoundSource(landmark.AudioFile, landmark.X, landmark.Y);
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        //Ustawienie gracza na środku ekranu
        _player = new Player { Rotation = 0 };

        _audioEngine = new AudioEngine();
        _audioEngine.Initialize();

        LoadMap("poziom1.json");
        

        //Pętla gry (ok. 60 FPS)
        _gameLoop = new DispatcherTimer();
        _gameLoop.Interval = TimeSpan.FromMilliseconds(16);
        _gameLoop.Tick += GameLoop_Tick;
        _gameLoop.Start();

        UpdatePlayerUI();
    }

    private void GameLoop_Tick(object? sender, EventArgs e)
    {
        //Obrót
        if (_isQPressed) _player.Rotation -= 3;
        if (_isEPressed) _player.Rotation += 3;

        //Przeliczenie na radiany
        double radians = _player.Rotation * Math.PI / 180.0;

        if (_isWPressed) //Krok w przód
        {
            _player.X += Math.Cos(radians) * Speed;
            _player.Y += Math.Sin(radians) * Speed;
        }
        if (_isSPressed) //Krok w tył
        {
            _player.X -= Math.Cos(radians) * Speed;
            _player.Y -= Math.Sin(radians) * Speed;
        }
        if (_isAPressed) //Krok w lewo
        {
            _player.X += Math.Cos(radians - Math.PI / 2) * Speed;
            _player.Y += Math.Sin(radians - Math.PI / 2) * Speed;
        }
        if (_isDPressed) //Krok w prawo
        {
            _player.X += Math.Cos(radians + Math.PI / 2) * Speed;
            _player.Y += Math.Sin(radians + Math.PI / 2) * Speed;
        }

        if (_currentMap != null)
        {
            if (_player.X < 10) _player.X = 10;
            if (_player.X > _currentMap.Width - 10) _player.X = _currentMap.Width - 10;
            if (_player.Y < 10) _player.Y = 10;
            if (_player.Y > _currentMap.Height - 10) _player.Y = _currentMap.Height - 10;
        }

        if (_currentMap != null)
        {
            string newTitle = $"X: {Math.Round(_player.X)} | Y: {Math.Round(_player.Y)} |";

            foreach (var landmark in _currentMap.Landmarks)
            {
                double deltaX = landmark.X - _player.X;
                double deltaY = landmark.Y - _player.Y;

                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                double absoluteAngle = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;

                double relativeAngle = absoluteAngle - _player.Rotation;
                relativeAngle = (relativeAngle + 540) % 360 - 180;

                newTitle += $"[{landmark.Name}: {Math.Round(distance)}m, {Math.Round(relativeAngle)}°] ";
            }

            Title = newTitle;
        }


        if (_isWPressed || _isSPressed || _isAPressed || _isDPressed || _isQPressed || _isEPressed)
        {
            UpdatePlayerUI();
        }

        if (_audioEngine != null)
        {
            _audioEngine.UpdateListener(_player.X, _player.Y, _player.Rotation);
        }
    }
     
        

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        UpdateKeyState(e.Key, true);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        UpdateKeyState(e.Key, false);
    }

    private void UpdateKeyState(Key key, bool isPressed)
    {
        switch (key)
        {
            case Key.W: _isWPressed = isPressed; break;
            case Key.S: _isSPressed = isPressed; break;
            case Key.A: _isAPressed = isPressed; break;
            case Key.D: _isDPressed = isPressed; break;
            case Key.Q: _isQPressed = isPressed; break;
            case Key.E: _isEPressed = isPressed; break;
        }
    }

    private void UpdatePlayerUI()
    {
        //Przesunięcie kropki na Canvasie
        Canvas.SetLeft(PlayerDot, _player.X - 10);
        Canvas.SetTop(PlayerDot, _player.Y - 10);

        //Rysowanie kreski wyznaczającej kierunek patrzenia
        double radians = _player.Rotation * Math.PI / 180.0;

        PlayerDirection.StartPoint = new Avalonia.Point(_player.X, _player.Y);

        double endX = _player.X + Math.Cos(radians) * 30;
        double endY = _player.Y + Math.Sin(radians) * 30;
        PlayerDirection.EndPoint = new Avalonia.Point(endX, endY);

        Title = $"Nawigacja Audio - X: {Math.Round(_player.X)} | Y: {Math.Round(_player.Y)} | Kąt: {_player.Rotation}°";

        //Pobranie aktualnego wymiaru okna do wyznaczenia jego środka
        double windowCenterX = Bounds.Width / 2;
        double windowCenterY = Bounds.Height / 2;

        //Wyśrodkowanie widoku
        MainCanvas.RenderTransform = new TranslateTransform(
            windowCenterX - _player.X,
            windowCenterY - _player.Y
        );
    }
}
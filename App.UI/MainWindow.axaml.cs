using Avalonia.Controls;
using Avalonia.Input;
using App.Core;
using System;
using Avalonia.Threading;

namespace App.UI;

public partial class MainWindow : Window
{

    private Player _player;
    private const double Speed = 2.0;//Prędkość chodzenia
    private DispatcherTimer _gameLoop;

    //Stany klawiszy
    private bool _isWPressed, _isSPressed, _isAPressed, _isDPressed;//poruszanie się WSAD
    private bool _isQPressed, _isEPressed;//Obrót QE

    public MainWindow()
    {
        InitializeComponent();

        //Ustawienie gracza na środku ekranu
        _player = new Player { X = 400, Y = 300, Rotation = 0 };

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

        if (_isWPressed || _isSPressed || _isAPressed || _isDPressed || _isQPressed || _isEPressed)
        {
            UpdatePlayerUI();
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
    }
}
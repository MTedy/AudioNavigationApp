using Avalonia.Controls;
using Avalonia.Input;
using App.Core;
using System;

namespace App.UI;

public partial class MainWindow : Window
{

    private Player _player;
    private const double Speed = 5.0;//Prędkość chodzenia

    public MainWindow()
    {
        InitializeComponent();

        //Ustawienie gracza na środku ekranu
        _player = new Player { X = 400, Y = 300, Rotation = 0 };

        UpdatePlayerUI();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        //Obrót gracza
        if (e.Key == Key.Q) _player.Rotation -= 5;
        if (e.Key == Key.E) _player.Rotation += 5;

        //Przeliczenie na radiany
        double radians = _player.Rotation * Math.PI / 180.0;

        //Chodzenie
        if (e.Key == Key.W) //Krok w przód
        {
            _player.X += Math.Cos(radians) * Speed;
            _player.Y += Math.Sin(radians) * Speed;
        }
        if (e.Key == Key.S) //Krok w tył
        {
            _player.X -= Math.Cos(radians) * Speed;
            _player.Y -= Math.Sin(radians) * Speed;
        }
        if (e.Key == Key.A) //Krok w lewo
        {
            _player.X += Math.Cos(radians - Math.PI / 2) * Speed;
            _player.Y += Math.Sin(radians - Math.PI / 2) * Speed;
        }
        if (e.Key == Key.D) //Krok w prawo
        {
            _player.X += Math.Cos(radians + Math.PI / 2) * Speed;
            _player.Y += Math.Sin(radians + Math.PI / 2) * Speed;
        }

        UpdatePlayerUI();
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
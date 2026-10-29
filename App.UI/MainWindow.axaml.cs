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
using System.Collections.Generic;


namespace App.UI;

public partial class MainWindow : Window
{

    private Map _currentMap;//Aktualnie wczytana mapa
    private AudioEngine _audioEngine;//Silnik dźwięku przestrzennego

    private Player _player;
    private const double Speed = 2.0;//Prędkość chodzenia
    private DispatcherTimer _gameLoop;

    //Stany klawiszy
    private bool _isWPressed, _isSPressed, _isAPressed, _isDPressed;//poruszanie się WSAD
    private bool _isQPressed, _isEPressed;//Obrót QE

    private int _bumpSoundId = -1;//Id dźwięku odtwarzanego przy zderzeniu ze ścianą

    private Dictionary<SoundLandmark, int> _loopingSounds = new Dictionary<SoundLandmark, int>();//Mapowanie punktu dźwiękowego na jego źródło w OpenAL

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
                //_audioEngine.AddLoopingSoundSource(landmark.AudioFile, landmark.X, landmark.Y);

                int sourceId = _audioEngine.AddLoopingSoundSource(landmark.AudioFile, landmark.X, landmark.Y);
                _loopingSounds.Add(landmark, sourceId);
            }

            if (_currentMap.Walls != null)
            {
                foreach (var wall in _currentMap.Walls)
                {
                    Line wallLine = new Line
                    {
                        StartPoint = new Avalonia.Point(wall.StartX, wall.StartY),
                        EndPoint = new Avalonia.Point(wall.EndX, wall.EndY),
                        Stroke = Brushes.Yellow,
                        StrokeThickness = 4
                    };

                    MainCanvas.Children.Add(wallLine);
                }
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

        _bumpSoundId = _audioEngine.LoadOneShotSound("bump.wav");

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

        //Zmienne na następną pozycję
        double targetX = _player.X;
        double targetY = _player.Y;

        if (_isWPressed) //Krok w przód
        {
            targetX += Math.Cos(radians) * Speed;
            targetY += Math.Sin(radians) * Speed;
        }
        if (_isSPressed) //Krok w tył
        {
            targetX -= Math.Cos(radians) * Speed;
            targetY -= Math.Sin(radians) * Speed;
        }
        if (_isAPressed) //Krok w lewo
        {
            targetX += Math.Cos(radians - Math.PI / 2) * Speed;
            targetY += Math.Sin(radians - Math.PI / 2) * Speed;
        }
        if (_isDPressed) //Krok w prawo
        {
            targetX += Math.Cos(radians + Math.PI / 2) * Speed;
            targetY += Math.Sin(radians + Math.PI / 2) * Speed;
        }

        //Sprawdzenie kolizji ze ścianami przed wykonaniem ruchu
        bool canMove = true;
        if (_currentMap?.Walls != null)
        {
            foreach (var wall in _currentMap.Walls)
            {
                if (LinesIntersect(_player.X, _player.Y, targetX, targetY, wall.StartX,  wall.StartY, wall.EndX, wall.EndY))
                {
                    canMove = false;
                    _audioEngine.PlaySound(_bumpSoundId);
                    break;
                }
            }
        }

        if (canMove)
        {
            _player.X = targetX;
            _player.Y = targetY;
        }

        //Ograniczenie gracza do granic mapy
        if (_currentMap != null)
        {
            if (_player.X < 10) _player.X = 10;
            if (_player.X > _currentMap.Width - 10) _player.X = _currentMap.Width - 10;
            if (_player.Y < 10) _player.Y = 10;
            if (_player.Y > _currentMap.Height - 10) _player.Y = _currentMap.Height - 10;
        }

        //Wyliczanie odległości i kąta do punktów dźwiękowych + aktualizacja tytułu okna
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

                if (_loopingSounds.TryGetValue(landmark, out int sourceId))
                {
                    //Sprawdzenie czy dźwięk jest zasłonięty ścianą (tłumienie + wirtualne przesunięcie źródła)
                    bool isMufled = false;

                    double virtualSoundX = landmark.X;
                    double virtualSoundY = landmark.Y;

                    if(_currentMap.Walls != null)
                    {
                        Wall closestIntersectingWall = null;
                        double minWallDist = double.MaxValue;

                        foreach (var wall in _currentMap.Walls)
                        {
                            if (LinesIntersect(_player.X, _player.Y, landmark.X, landmark.Y, wall.StartX, wall.StartY, wall.EndX, wall.EndY))
                            {
                                isMufled = true;

                                //Wylicznie środka ściany do oceny jej odległości od gracza
                                double midX = (wall.StartX + wall.EndX) / 2;
                                double midY = (wall.StartY + wall.EndY) / 2;
                                double distToWall = Math.Sqrt(Math.Pow(midX - _player.X, 2) + Math.Pow(midY - _player.Y, 2));

                                if (distToWall < minWallDist)
                                {
                                    minWallDist = distToWall;
                                    closestIntersectingWall = wall;
                                }                                
                            }
                        }
                        
                        if (closestIntersectingWall != null)
                        {
                            var wall = closestIntersectingWall;

                            //Obliczanie dyfracji
                            double distToStart = Math.Sqrt(Math.Pow(wall.StartX - _player.X, 2) + Math.Pow(wall.StartY - _player.Y, 2));
                            double distToEnd = Math.Sqrt(Math.Pow(wall.EndX - _player.X, 2) + Math.Pow(wall.EndY - _player.Y, 2));

                            //Sprawdzanie warunku czy koniec ściany nie wbija sie w inną ścianę
                            bool isStartFree = IsEndpointFree(wall.StartX, wall.StartY, _currentMap.Walls, wall);
                            bool isEndFree = IsEndpointFree(wall.EndX, wall.EndY, _currentMap.Walls, wall);

                            //Wektor kierunkowy ściany
                            double wallDx = wall.EndX - wall.StartX;
                            double wallDy = wall.EndY - wall.StartY;
                            double wallLen = Math.Sqrt(wallDx * wallDx + wallDy * wallDy);

                            if (wallLen > 0 && (isStartFree || isEndFree))
                            {
                                //Znormalizowany wektor
                                double nx = wallDx / wallLen;
                                double ny = wallDy / wallLen;

                                double offset = 50.0;

                                //Logika wyboru "lepszego" końca
                                bool chooseStart = false;
                                if (isStartFree && isEndFree)
                                {
                                    chooseStart = distToStart < distToEnd; //Oba wolne - bierzemy bliższy
                                }
                                else if (isStartFree) chooseStart = true; //Tylko start jest wolny
                                else chooseStart = false; //Tylko koniec jest wolny

                                if (chooseStart)
                                {
                                    virtualSoundX = wall.StartX - nx * offset;
                                    virtualSoundY = wall.StartY - ny * offset;
                                }
                                else
                                {
                                    virtualSoundX = wall.EndX + nx * offset;
                                    virtualSoundY = wall.EndY + ny * offset;
                                }
                            }
                        }
                    }

                    _audioEngine.SetSourceVolume(sourceId, isMufled ? 0.2f : 1.0f);
                    _audioEngine.UpdateSourcePosition(sourceId, virtualSoundX, virtualSoundY);
                }
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

    //Zapisanie stanu wciśnięcia/zwolnienia klawisza ruchu lub obrotu
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

        Title = $"Nawigacja Audio - X: {Math.Round(_player.X)} | Y: {Math.Round(_player.Y)} | Kąt: {_player.Rotation}° | Ściany: {_currentMap.Walls?.Count ?? 0}";

        //Pobranie aktualnego wymiaru okna do wyznaczenia jego środka
        double windowCenterX = Bounds.Width / 2;
        double windowCenterY = Bounds.Height / 2;

        //Wyśrodkowanie widoku
        MainCanvas.RenderTransform = new TranslateTransform(
            windowCenterX - _player.X,
            windowCenterY - _player.Y
        );
    }

    //Sprawdzenie przecięcia dwóch odcinków - wykrywanie kolizji ze ścianą i zasłonięcia dźwięku
    private bool LinesIntersect(double p1x, double p1y, double p2x, double p2y, double p3x, double p3y, double p4x, double p4y)
    {
        double denominator = (p4y - p3y) * (p2x - p1x) - (p4x - p3x) * (p2y - p1y);

        if (Math.Abs(denominator) < 0.0001)
            return false;

        double ua = ((p4x - p3x) * (p1y - p3y) - (p4y - p3y) * (p1x - p3x)) / denominator;
        double ub = ((p2x - p1x) * (p1y - p3y) - (p2y - p1y) * (p1x - p3x)) / denominator;

        return (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1);
    }

    //Sprawdzanie czy koniec ściany jest wolny
    private bool IsEndpointFree (double px, double py, List<Wall> allWalls, Wall currentWall)
    {
        foreach (var wall in allWalls)
        {
            if (wall == currentWall) continue;

            double distToStart = Math.Sqrt(Math.Pow(px - wall.StartX, 2) + Math.Pow(py - wall.StartY, 2));
            double distToEnd = Math.Sqrt(Math.Pow(px - wall.EndX, 2) + Math.Pow(py - wall.EndY, 2));

            if (distToStart < 1.0 || distToEnd < 1.0)
            {
                continue; //Narożnik w kształcie litery L
            }

            double wallLenght = Math.Sqrt(Math.Pow(wall.EndX - wall.StartX, 2) + Math.Pow(wall.EndY  - wall.StartY, 2));

            if (Math.Abs((distToStart + distToEnd) - wallLenght) < 1.0)
            {
                return false; //Ściany łączą się tworząc literę T
            }
        }
        return true; //Punkt jest wolny
    }
}
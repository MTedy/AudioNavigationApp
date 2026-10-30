# Audio Navigation App (Symulator Nawigacji Audio)

Aplikacja inżynierska będąca symulatorem nawigacji przestrzennej w oparciu o dźwięk, zaprojektowana z myślą o osobach niewidomych. Projekt stawia na wierne oddanie właściwości akustycznych środowiska, pozwalając użytkownikowi na budowanie "mapy mentalnej" wirtualnych pomieszczeń bez użycia wzroku.

## 🚀 Główne funkcje (MVP)

* **Wczytywanie topografii z JSON:** Ściany oraz punkty nawigacyjne (Landmarks) są wczytywane dynamicznie z zewnętrznego pliku (np. `poziom1.json`).
* **Detekcja kolizji:** Twarda blokada ruchu przy kontakcie gracza z przeszkodą.
* **Audio Feedback:** Natychmiastowe sprzężenie zwrotne (one-shot audio) informujące o uderzeniu w ścianę.
* **Okluzja akustyczna:** Realistyczne tłumienie głośności źródeł dźwięku, gdy gracz i cel oddzieleni są od siebie przeszkodą.
* **Uproszczona dyfrakcja:** Inteligentny algorytm raycastingu – gdy cel jest zasłonięty, wirtualne źródło dźwięku płynnie przesuwa się na najbliższą wolną krawędź ściany (omijając ślepe zaułki), naturalnie naprowadzając gracza na światło przejścia.

## 🎧 Wymagania

* **Słuchawki stereofoniczne** – absolutnie niezbędne do prawidłowego odbioru pozycjonowania przestrzennego (OpenAL).
* System operacyjny Windows (64-bit).

## ⌨️ Sterowanie

* **W / S** – Ruch do przodu / do tyłu
* **A / D** – Krok w lewo / w prawo (strafing)
* **Q / E** – Obrót w lewo / w prawo

## 📥 Jak uruchomić

1. Przejdź do zakładki [Releases](../../releases) i pobierz najnowszą wersję (plik `publish.zip`).
2. Wypakuj archiwum.
3. Upewnij się, że w folderze z plikiem wykonywalnym `.exe` znajdują się pliki dźwiękowe `.wav` oraz plik konfiguracyjny `poziom1.json`.
4. Uruchom aplikację.

## 🛠️ Technologie

* **Język:** C# (.NET)
* **Interfejs / Framework:** Avalonia UI
* **Silnik Audio:** OpenAL


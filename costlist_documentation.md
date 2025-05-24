# Dokumentacja systemu zarządzania zadaniami i sprzętem

## Przegląd systemu

System Tasks to kompleksowe narzędzie do zarządzania i przydziału zadań na różne komponenty sprzętowe. Umożliwia modelowanie grafów zadań, definiowanie sprzętu, optymalizację kosztów i czasu wykonania oraz symulację przetwarzania zadań.

## Główne klasy i komponenty

### 1. Edge - Krawędź grafu

Reprezentuje połączenie między wierzchołkami w grafie zadań.

**Właściwości:**
- `v1`, `v2` - identyfikatory wierzchołków (zadań)
- `weight` - waga krawędzi (domyślnie 1)

**Metody:**
- `GetV1()`, `GetV2()`, `GetWeight()` - gettery dla właściwości

### 2. Graf - Graf zadań

Implementuje graf skierowany z listą sąsiedztwa do reprezentacji zależności między zadaniami.

**Główne metody:**
- `AddEdge(int v1, int v2, int weight = 1)` - dodaje krawędź między zadaniami
- `CheckEdge(int v1, int v2)` - sprawdza czy istnieje krawędź
- `GetOutNeighbourIndices(int vertex)` - zwraca listę następników
- `BFS()` - przeszukiwanie wszerz
- `DFS(int start, int end)` - znajdowanie wszystkich ścieżek między zadaniami

### 3. Hardware - Sprzęt

Reprezentuje komponenty sprzętowe dostępne w systemie.

**Typy sprzętu (HardwareType):**
- `HC` (Hardware Core) - rdzeń sprzętowy
- `PE` (Processing Element) - element przetwarzający

**Właściwości:**
- `cost` - koszt użycia sprzętu
- `type` - typ sprzętu
- `id` - unikalny identyfikator

### 4. COM - Kanał komunikacyjny

Reprezentuje kanały komunikacyjne łączące komponenty sprzętowe.

**Właściwości:**
- `bandwidth` - przepustowość kanału
- `cost` - koszt użycia kanału
- `connectedHardware` - zbiór podłączonych komponentów sprzętowych

**Metody:**
- `AddHardware(Hardware hw)` - dodaje sprzęt do kanału
- `IsConnected(Hardware hw)` - sprawdza czy sprzęt jest podłączony

### 5. Instance - Instancja sprzętu

Reprezentuje konkretną instancję sprzętu z przypisanymi zadaniami.

**Właściwości:**
- `hardware` - referencja do komponentu sprzętowego
- `taskSet` - zbiór przypisanych zadań

**Metody:**
- `AddTask(int taskId)` - przypisuje zadanie do instancji
- `RemoveTask(int taskId)` - usuwa zadanie z instancji

### 6. TimeAndCost - Czas i koszt wykonania

Przechowuje informacje o czasie i koszcie wykonania zadań na różnym sprzęcie.

**Właściwości:**
- `Times` - lista czasów wykonania (dla podzadań)
- `Costs` - lista kosztów wykonania (dla podzadań)

### 7. Times - Macierz czasów i kosztów

Zarządza macierzą czasów i kosztów wykonania wszystkich zadań na wszystkich dostępnych komponentach sprzętowych.

**Główne metody:**
- `SetRandomTimesAndCosts()` - generuje losowe czasy i koszty
- `SetTimesMatrix()`, `SetCostsMatrix()` - ustawia macierze z danych
- `GetTimes(int taskId, Hardware hw)` - pobiera czasy dla zadania
- `GetCosts(int taskId, Hardware hw)` - pobiera koszty dla zadania

### 8. CostList - Główna klasa systemu

Centralna klasa zarządzająca całym systemem - zadaniami, sprzętem, przydziałem i symulacją.

## Główne funkcjonalności

### Wczytywanie danych z pliku

```csharp
int LoadFromFile(string filename, out string errorMessage)
```

Wczytuje definicję systemu z pliku tekstowego zawierającego sekcje:
- `@tasks` - definicja zadań i ich zależności
- `@proc` - definicja komponentów sprzętowych
- `@times` - macierz czasów wykonania
- `@cost` - macierz kosztów wykonania
- `@comm` - definicja kanałów komunikacyjnych

### Generowanie losowych danych

```csharp
void RandALL()
```

Generuje losowy system zawierający:
- Graf zadań z losowymi zależnościami
- Komponenty sprzętowe o losowych parametrach
- Macierz czasów i kosztów
- Kanały komunikacyjne

### Strategie przydziału zadań

#### 1. Przydział do najszybszego sprzętu
```csharp
void AssignEachToFastestHardware()
```
Każde zadanie przydziela do sprzętu zapewniającego najkrótszy czas wykonania.

#### 2. Przydział do najtańszego sprzętu
```csharp
void AssignToCheapestHardware()
```
Wszystkie zadania przydziela do sprzętu o najniższym łącznym koszcie.

#### 3. Przydział według reguły
```csharp
void taskDistribution(int rule)
```
- `rule = 0` - optymalizacja czasu
- `rule = 1` - optymalizacja kosztu

### Symulacja wykonania

```csharp
void RunTasks()
```

Przeprowadza symulację wykonania zadań z uwzględnieniem:
- Zależności między zadaniami
- Równoległego wykonania podzadań
- Rzeczywistych czasów wykonania
- Śledzenia postępu

### Zarządzanie instancjami

System automatycznie tworzy i zarządza instancjami sprzętu:

```csharp
void createInstance(int taskId, Hardware hw)
void addTaskToInstance(int taskId, Instance inst)
void removeTaskFromInstance(int taskId)
```

### Analiza wydajności

System dostarcza metryk wydajności:

```csharp
Instance GetLongestRunningInstance()  // Najdłużej działająca instancja
Instance GetShortestRunningInstance() // Najkrócej działająca instancja
int GetIdleTime(Instance inst, int timeStop) // Czas bezczynności
```

## Format pliku wejściowego

### Sekcja @tasks
```
@tasks
T0 2 1(5) 2(3)  // Zadanie 0 ma 2 następników: zadanie 1 (waga 5), zadanie 2 (waga 3)
T1 1 3(2)       // Zadanie 1 ma 1 następnik: zadanie 3 (waga 2)
```

### Sekcja @proc
```
@proc 4
10 1 0  // Koszt: 10, Typ: HC, ID: 0
15 2 1  // Koszt: 15, Typ: PE, ID: 1
```

### Sekcja @times
```
@times
50 60 40    // Czasy wykonania zadania 0 na różnym sprzęcie
[10 15 20] 30 25  // Zadanie z podzadaniami
```

### Sekcja @cost
```
@cost
5 8 3       // Koszty wykonania zadania 0
[2 3 4] 6 2 // Koszty dla zadania z podzadaniami
```

### Sekcja @comm
```
@comm 2
CHAN0 50 100 1 1 0  // Kanał 0: koszt 50, przepustowość 100, połączony ze sprzętem 0 i 1
CHAN1 30 80 0 1 1   // Kanał 1: koszt 30, przepustowość 80, połączony ze sprzętem 1 i 2
```

## Przykład użycia

```csharp
// Utworzenie systemu
var costList = new CostList(5, 2, 3, 2, 10); // 5 zadań, 2 HC, 3 PE, 2 kanały

// Wczytanie z pliku
string error;
if (costList.LoadFromFile("system.txt", out error) == 1)
{
    // Przydział do najszybszego sprzętu
    costList.AssignEachToFastestHardware();
    
    // Wyświetlenie przydziału
    costList.PrintInstances();
    
    // Symulacja wykonania
    costList.RunTasks();
}

// Lub generowanie losowego systemu
costList.RandALL();
costList.taskDistribution(0); // Optymalizacja czasu
costList.PrintALL("output.txt", true);
```

## Obsługa podzadań

System obsługuje zadania złożone z podzadań, które mogą być wykonywane równolegle:

- **Czas wykonania**: maksimum z czasów podzadań (wykonanie równoległe)
- **Koszt wykonania**: suma kosztów wszystkich podzadań
- **Format**: `[czas1 czas2 czas3]` lub `[koszt1 koszt2 koszt3]`

## Metryki i analiza

System dostarcza szczegółowych informacji o:
- Czasie krytycznej ścieżki
- Wykorzystaniu sprzętu
- Czasach bezczynności
- Łącznych kosztach
- Harmonogramie wykonania zadań

## Uwagi implementacyjne

- System używa wielowątkowości do symulacji równoległego wykonania
- Wspiera skalowalność czasową symulacji
- Implementuje mechanizmy synchronizacji dla bezpiecznego dostępu do współdzielonych danych
- Obsługuje błędy wczytywania i walidację danych wejściowych
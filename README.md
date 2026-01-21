# HexaGo - Playable Ads

Интерактивная игра "HexaGo - сортировочный пазл" для платформы Luna Playworks. Проект демонстрирует архитектуру модульной игры на базе Unity с использованием современных паттернов проектирования и асинхронного программирования.

## 📋 Описание

HexaGo - это головоломка, где игрокам необходимо сортировать и комбинировать цветные шестиугольники. Проект разработан для Luna Playworks и содержит полную архитектуру игры с DI контейнером, реактивным программированием и MVP паттерном для UI.

## 🎮 Особенности

- **Модульная архитектура** - легко расширяемая структура кода
- **Dependency Injection** - управление зависимостями через Zenject
- **Реактивное программирование** - использование R3 для событий и состояний
- **Асинхронные операции** - работа с UniTask
- **Управление жизненным циклом** - JetBrains.Lifetimes для корректного управления ресурсами
- **Анимации** - PrimeTween для плавных переходов
- **MVP паттерн** - чистое разделение логики UI и бизнеса

## 🛠️ Технологический стек

| Компонент | Версия | Назначение |
|-----------|--------|-----------|
| **Unity** | 2022+ | Игровой движок |
| **Zenject** | Latest | Dependency Injection контейнер |
| **UniTask** | Latest | Асинхронное программирование |
| **R3** | Latest | Реактивное программирование |
| **JetBrains.Lifetimes** | Latest | Управление жизненным циклом |
| **PrimeTween** | Latest | Анимации и Tweens |
| **TextMesh Pro** | 3.0.7 | UI текст |
| **Luna Playworks** | 7.0.0 | Интеграция с платформой |

## 📁 Структура проекта

```
Assets/Game/
├── Scripts/
│   ├── Gameplay/
│   │   ├── Configs/              # ScriptableObject конфигурации
│   │   │   ├── GameplayConfig.cs     # Параметры геймплея
│   │   │   ├── GridConfig.cs         # Конфигурация сетки
│   │   │   ├── HexColorConfig.cs     # Цветовые схемы
│   │   │   └── PrefabReferences.cs   # Ссылки на префабы
│   │   │
│   │   ├── HexGrid/              # Модели данных и компоненты сетки
│   │   │   ├── HexCoord.cs           # Координаты шестиугольников
│   │   │   ├── HexPiece.cs           # Визуальное представление фишки
│   │   │   ├── HexStack.cs           # Стопка фишек
│   │   │   ├── GridCell.cs           # Ячейка сетки
│   │   │   ├── CellBackground.cs     # Фон ячейки
│   │   │   └── GridShapeGenerator.cs # Генератор форм сетки
│   │   │
│   │   ├── Services/             # Игровые сервисы
│   │   │   ├── GameStateService.cs   # Управление состоянием игры
│   │   │   ├── GridService.cs        # Логика работы с сеткой
│   │   │   ├── DragDropService.cs    # Механика перетаскивания
│   │   │   ├── StackFactory.cs       # Создание стопок
│   │   │   ├── MergeService.cs       # Логика слияния фишек
│   │   │   ├── PlayerStacksService.cs # Управление стопками игрока
│   │   │   ├── LevelGenerator.cs     # Генерация уровней
│   │   │   └── TutorialService.cs    # Управление туториалом
│   │   │
│   │   ├── HexaGoBootstrap.cs        # Инициализация игры
│   │   ├── HexaGoSceneInstaller.cs   # Scene-level DI
│   │   ├── HexaGoServicesInstaller.cs # Services DI
│   │   ├── HexaGoPoolInstaller.cs    # Object Pool DI
│   │   └── README.md                 # Архитектурная документация
│   │
│   ├── View/                     # UI компоненты
│   │   └── Gameplay/
│   │       ├── Tutorial/             # Экран туториала
│   │       ├── Packshot/             # Packshot интерактор
│   │       └── Core/                 # Базовые классы MVP
│   │
│   └── Utilities/               # Утилиты
│       ├── Lifetimes/               # Помощники для управления жизненным циклом
│       └── Addressables/            # Работа с Addressables
│
├── Prefabs/                      # Игровые префабы
│   ├── GridCell.prefab
│   ├── HexPiece.prefab
│   ├── HexStack.prefab
│   └── CellBackground.prefab
│
├── ScriptableObjects/            # Конфигурационные ассеты
│   ├── GameplayConfig.asset
│   ├── GridConfig.asset
│   └── HexColorConfig.asset
│
├── Scenes/
│   └── SampleScene.unity         # Основная игровая сцена
│
├── Sprites/                      # Графические ассеты
├── Materials/                    # Материалы
└── Shaders/                      # Шейдеры
```

## 🚀 Установка и запуск

### Требования

- **Unity 2022 LTS или выше**
- **.NET Framework 4.7.1 или выше**
- **Visual Studio или Rider** (рекомендуется Rider)
- **Luna Playworks SDK** (для сборки)

### Установка

1. **Клонируйте репозиторий**
   ```bash
   git clone https://github.com/your-username/hexago-playable-ads.git
   cd hexago-playable-ads
   ```

2. **Откройте проект в Unity**
   - Запустите Unity Hub
   - Добавьте проект из папки `hexago-playable-ads`
   - Дождитесь импорта всех ассетов и компиляции

3. **Установите зависимости (если требуется)**
   - Проект использует NuGet пакеты через NuGetForUnity
   - Они должны установиться автоматически

### Запуск в редакторе

1. Откройте сцену `Assets/Game/Scenes/SampleScene.unity`
2. Нажмите Play в Unity Editor
3. Используйте мышь для перетаскивания фишек

## 🏗️ Архитектура

### Паттерны проектирования

#### MVP (Model-View-Presenter)
```
IProtocol (View Layer)
    ↓
IInteractor (Presenter Layer)
    ↓
Services & Models (Model Layer)
```

**Пример:**
- `HexaPackshotProtocol` - интерфейс UI
- `HexaPackshotInteractor` - логика и обработка событий
- `MonoPresenter` - визуальное представление

#### Dependency Injection
Управление зависимостями через **Zenject**:

```csharp
public class HexaGoServicesInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IGameStateService>().To<GameStateService>().AsSingle();
        Container.Bind<IGridService>().To<GridService>().AsSingle();
        // ...
    }
}
```

#### Реактивное программирование
Использование **R3** для состояний и событий:

```csharp
_gameStateService.CurrentState.Advise(lifetime, state =>
{
    protocol.IsVisible.Value = state == GameState.Packshot;
});
```

### Game States (Состояния игры)

```
Loading → Menu → Gameplay → Packshot → GameEnded
```

- **Loading** - загрузка ассетов и инициализация
- **Menu** - меню игры
- **Gameplay** - активная игра
- **Packshot** - экран завершения (CTA)
- **GameEnded** - игра завершена

## 🔧 Конфигурация

Основные параметры находятся в `ScriptableObjects`:

- **GameplayConfig.asset** - параметры геймплея (сложность, времени и т.д.)
- **GridConfig.asset** - размер сетки, количество цветов
- **HexColorConfig.asset** - цветовые схемы для фишек

Редактируйте эти ассеты через Unity Inspector для быстрого тестирования разных конфигураций.

## 🐛 Отладка

### Luna Playable специфика

Проект содержит условную компиляцию для Luna Playable:

```csharp
#if LUNA_PLAYABLE
    Luna.Unity.LifeCycle.GameEnded();
    Luna.Unity.Playable.InstallFullGame();
#endif
```

Для отключения Luna функций во время разработки используйте обычный Play Mode в Unity Editor.

### Для других платформ

1. File → Build Settings
2. Выберите целевую платформу
3. Отключите LUNA_PLAYABLE флаг в Player Settings
4. Нажмите Build

## 📄 Лицензия

Этот проект под лицензией MIT. Подробнее смотрите [LICENSE](LICENSE) файл.

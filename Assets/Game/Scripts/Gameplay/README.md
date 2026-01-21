# HexaGo - Сортировочный пазл

Клон игры "HexaGo - сортировочный пазл" для Luna Playworks.

## Архитектура

Проект использует:
- **Zenject** - DI контейнер
- **UniTask** - асинхронные операции
- **JetBrains.Lifetimes** - управление жизненным циклом
- **R3** - реактивное программирование
- **PrimeTween** - анимации
- **MVP** - паттерн для UI (IProtocol, IInteractor, MonoPresenter)

## Структура проекта

```
Assets/Game/Scripts/
├── Gameplay/
│   ├── Configs/           # ScriptableObject конфигурации
│   │   ├── GameplayConfig.cs
│   │   ├── GridConfig.cs
│   │   ├── HexColorConfig.cs
│   │   └── PrefabReferences.cs
│   ├── HexGrid/           # Модели данных и визуальные компоненты
│   │   ├── HexCoord.cs
│   │   ├── HexPiece.cs
│   │   ├── HexStack.cs
│   │   ├── GridCell.cs
│   │   ├── CellBackground.cs
│   │   └── GridShapeGenerator.cs
│   ├── Services/          # Игровые сервисы
│   │   ├── GridService.cs
│   │   ├── StackFactory.cs
│   │   ├── MergeService.cs
│   │   ├── DragDropService.cs
│   │   ├── PlayerStacksService.cs
│   │   ├── LevelGenerator.cs
│   │   ├── GameStateService.cs
│   │   └── TutorialService.cs
│   ├── HexaGoBootstrap.cs
│   ├── HexaGoSceneInstaller.cs
│   ├── HexaGoServicesInstaller.cs
│   └── HexaGoPoolInstaller.cs
└── View/
    └── Gameplay/
        ├── Tutorial/      # UI туториала
        │   ├── HexaTutorialUIProtocol.cs
        │   ├── HexaTutorialUIInteractor.cs
        │   └── HexaTutorialUIPresenter.cs
        └── Packshot/      # UI пэкшота
            ├── HexaPackshotProtocol.cs
            ├── HexaPackshotInteractor.cs
            └── HexaPackshotPresenter.cs
```

## Настройка сцены

### 1. Создание SceneContext

1. Создайте пустой GameObject "SceneContext"
2. Добавьте компонент `SceneContext`
3. Добавьте компонент `HexaGoSceneInstaller`

### 2. Настройка HexaGoSceneInstaller

Присвойте ссылки:
- **Gameplay Config** - Assets/Game/ScriptableObjects/GameplayConfig
- **Grid Config** - Assets/Game/ScriptableObjects/GridConfig
- **Hex Color Config** - Assets/Game/ScriptableObjects/HexColorConfig
- **Grid Container** - пустой Transform для ячеек сетки
- **Player Stacks Container** - пустой Transform для стопок игрока
- **Main Camera** - основная камера сцены

### 3. Настройка конфигов

#### GameplayConfig
- Настройте ссылки на префабы:
  - HexPiece - Assets/Game/Prefabs/HexPiece
  - HexStack - Assets/Game/Prefabs/HexStack
  - GridCell - Assets/Game/Prefabs/GridCell
  - CellBackground - Assets/Game/Prefabs/CellBackground

#### GridConfig
- Выберите форму сетки (Hexagon, Rectangle, Diamond, Triangle)
- Настройте размеры и цвета

#### HexColorConfig
- Настройте доступные цвета гексов

### 4. Настройка ProjectContext

Убедитесь, что ProjectContext (Assets/Game/Resources/ProjectContext.prefab) содержит:
- `LifetimeInstaller`
- `PooledAddressablesInstaller`
- `ViewCoreInstaller`

### 5. Настройка UI

1. Создайте Canvas для UI
2. Добавьте `HexaTutorialUIPresenter` для руки туториала
3. Добавьте `HexaPackshotPresenter` для пэкшота

## Игровой процесс

1. **Загрузка** - генерация сетки и начальных стопок
2. **Туториал** - показ руки с указанием куда перетащить стопку
3. **Игра** - Drag&Drop стопок на поле
4. **Слияние** - автоматическое слияние стопок одного цвета
5. **Пэкшот** - показ экрана завершения

## Luna SDK интеграция

При сборке для Luna Playworks:
- Определите `LUNA_PLAYABLE` в Player Settings
- Пэкшот автоматически вызовет `Luna.Unity.LifeCycle.GameEnded()` и `Luna.Unity.Playable.InstallFullGame()`

# Cosmic Warden — GAME2014 Assignment 1

**Student:** Naz Yagmur  ·  **Course:** GAME2014 – Mobile Game Development I  ·  **Engine:** Unity 6 (6000.6), C#

A 2D vertical-scrolling arcade shooter for mobile (portrait). Drag with one finger to fly; the ship fires while you touch the screen. Shoot Raiders and Saucers, collect loot and get a high score.

## Screens
MainMenu → Instructions → Play → GameOver (Play Again / Main Menu)

## Features
| Requirement | Implementation |
|---|---|
| Touch control + weapon | `PlayerController` – drag to move, fires while touching |
| Enemy AI | `ChaserEnemy` – detection radius + seek steering + dash · `ShooterEnemy` – line-of-sight raycast + vision cone, strafing |
| Random loot | `PickupSpawner` – random coins and stars at random positions |
| Score / lives | `GameManager` events → `HUDController` (3 lives) |
| Sound & music | `AudioManager` singleton, per-scene `SceneMusic` |
| Animation | Animator clips for ship thrust, enemies, explosion, coin, star; animated UI |

## Design patterns (course weeks 5–6)
- **Object Pool** – `BulletPool` reuses bullets instead of Instantiate/Destroy.
- **Factory Method** – `EnemyFactory` (creator) → `ChaserFactory`, `ShooterFactory` (concrete creators) build `EnemyBase` products; `EnemySpawner` is the client.

## Setup
Open in Unity, then menu **GAME2014 › Build Game Scenes** (`Assets/Editor/GameBuilder.cs`) to (re)generate prefabs, animations and the four scenes.

## Asset credits
- All sprites, sound effects and music are original, generated procedurally by the author's scripts in `AssetTools/` (palette: Sweetie 16, CC0).
- Font: *Press Start 2P* by CodeMan38 – SIL Open Font License (`Assets/Fonts/PressStart2P-OFL.txt`).

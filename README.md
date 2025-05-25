# CipherJourney

**CipherJourney** is a browser-based puzzle game where players must decode encrypted sentences using logic and deduction. The game focuses on word-level cipher solving and offers a new challenge every day.

## Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [Installation Instructions](#installation-instructions)
4. [Usage Guide](#usage-guide)
5. [Future Improvements](#future-improvements)
6. [License](#license)

## Overview

CipherJourney challenges players to decrypt a ciphered sentence by guessing words one at a time. It’s inspired by games like *Wordle*, but instead of guessing letters, players must uncover full words encrypted by classic cipher algorithms.

This personal project helped me understand core principles of:
- ASP.NET Core MVC architecture
- Database schema design and EF Core migrations
- Cookie-based authentication and game state management
- Data protection and server-client interaction
- Dynamic frontend interactions using AJAX
- Good practices for designing extendable systems

## Features

- 🔐 **Login and account system** — with email verification and password recovery
- 📆 **Daily challenge** — one encrypted sentence each day, shared among all users
- 🧠 **Points system** — earn points based on how quickly (in how few guesses) you solve the puzzle
- 📊 **Leaderboard** — see top 10 players and your rank
- 💾 **Persistent game state** — progress is saved in cookies and validated server-side
- 🧩 **Multiple classic ciphers supported** — such as Caesar and Atbash

## Installation Instructions

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- SQL Server or SQL Server Express (localdb is also supported)
- Visual Studio or another IDE like JetBrains Rider (optional)

### Setup

1. Clone the repository:
```bash
git clone https://github.com/<your-username>/CipherJourney.git
cd CipherJourney
```

2. Create an `appsettings.json` file.

3. Copy over the following into `appsettings.json`:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YourSqlServerName;Initial Catalog=CipherJourney;Integrated Security=True;Trust Server Certificate=True"
  },
  "Email": {
    "CipherJourney": "youemail@etc.com",
    "AppPass": "your app pass here"
  }
}
```
4. Configure the connection string in `appsettings.json` to point to your SQL Server instance.

5. Configure your 'AppPass' in your google account and put it in the `AppPass` section in `appsettings.json`.

6. Apply all migrations:
```bash
dotnet ef database update
```

> This will automatically create the schema and seed required data (cipher types, example sentences, etc.)

4. Run the application:
```bash
dotnet run
```

Then open `http://localhost:<port>` in your browser.

## Usage Guide

- Register an account and verify your email.
- Each day, you'll receive a new encrypted sentence.
- Guess one word at a time to try to reveal the original sentence.
- Points are awarded based on how few guesses it takes to solve the challenge.
- You can view your account details, stats, and the global leaderboard.

## Future Improvements

Some planned enhancements to the project include:

- ⏱ **Timed Mode** — A game mode where players compete to solve the sentence as fast as possible.
- 💡 **Hint System** — Players will be able to spend points or guesses to reveal partial words or clues.
- 🌍 **Multiplayer Sharing** — Users will be able to create their own encrypted sentence and challenge friends with a shareable link.
- 🌐 **Localization Support** — Add support for multiple languages including Bulgarian, and allow daily challenges in different languages.
- 📱 **Mobile-friendly UI** — Improve responsive design and accessibility for mobile users.

## License

This project is licensed under the MIT License.

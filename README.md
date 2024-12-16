# CipherJourney

CipherJourney is a fun and challenging puzzle game where players decode clues, crack ciphers, and solve intricate puzzles to progress. Inspired by games like Wordle, CipherJourney combines entertainment with brain-teasing challenges, offering users a unique and enjoyable experience.

## Table of Contents
1. [Introduction](#introduction)
2. [Features](#features)
3. [Installation Instructions](#installation-instructions)
4. [Usage Guide](#usage-guide)
5. [Screenshots](#screenshots)
6. [Contributing](#contributing)
7. [License](#license)

## Introduction

CipherJourney challenges players to think critically and solve puzzles in a fun, interactive way. Each level presents a new cipher or puzzle, with increasing difficulty, making it perfect for both casual gamers and puzzle enthusiasts.

### Core Objectives
* Provide an engaging, puzzle-solving experience.
* Create a balance of challenge and fun for all age groups.
* Encourage players to learn basic cryptography concepts while playing.

## Features
* **Daily Cipher Challenge**: A new puzzle each day to test your skills.
* **Player Progression**: Track your achievements, scores, and milestones.
* **Verification System**: Ensure account security with email verification.
* **Intuitive Interface**: Simple and clean UI for seamless gameplay.

## Installation Instructions

### Prerequisites
* **.NET 6.0 SDK** or later
* **SQL Server** or compatible database
* Basic understanding of C# and ASP.NET Core

### Setup Instructions
1. Clone the repository:
```bash
git clone https://github.com/<your-username>/CipherJourney.git
cd CipherJourney
```

2. Install dependencies:
```bash
dotnet restore
```

3. Set up the database:
   * Update the `appsettings.json` file with your database connection string.
   * Run the migrations:
```bash
dotnet ef database update
```

4. Run the project:
```bash
dotnet run
```

5. Open the application in your browser at `http://localhost:<port>`

## Usage Guide
1. **Sign Up and Verify**:
   * Register with your email and verify your account to start playing.
2. **Solve Puzzles**:
   * Choose a daily challenge or previous puzzles.
   * Enter solutions and progress through levels.
3. **Track Progress**:
   * Monitor scores and achievements on your profile page.

## Contributing
We welcome contributions from the community! To contribute:
1. Fork the repository.
2. Create a new branch for your feature or bug fix:
```bash
git checkout -b feature/your-feature-name
```

3. Make your changes and commit them:
```bash
git commit -m "Add your message here"
```

4. Push your changes and create a pull request.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.

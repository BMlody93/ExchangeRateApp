Exchange Rate App

This is a full-stack web application for comparing exchange rates between currencies.  
It consists of a **.NET 8 Web API backend** and an **Angular frontend**.

---

How to Run the Project Locally

Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+ recommended)](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli):  

Install it globally if you haven’t already:
  ```bash
  npm install -g @angular/cli
  ```
1. Run the Backend

    Open a terminal in the backend project directory:
  ```bash
  cd ExchangeRateApp.WebApi
  ```
Run the API:
  ```bash
  dotnet run
  ```
The API should be available at:

  https://localhost:7061

2. Run the Frontend

    Open a terminal in the frontend directory:
  ```bash
  cd exchange-rate-app-frontend
  ```
Install dependencies (first run only):
  ```bash
  npm install
  ```
Start the Angular development server:
  ```bash
  npm start
  ```
The app should be available at:

  http://localhost:58883

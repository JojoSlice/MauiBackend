# MauiBackend

A trading simulation backend API built with ASP.NET Core 8.0 that powers a competitive stock trading platform. Users compete in seasonal trading competitions using a virtual points system.

## Overview

MauiBackend is a RESTful API that enables users to:
- Register and authenticate with JWT tokens
- Trade stocks with long and short positions
- Compete in time-bound trading seasons
- Track profit/loss metrics
- Manage portfolios with stop-loss and take-profit orders

## Technology Stack

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: MongoDB Atlas
- **Authentication**: JWT Bearer Tokens
- **Password Security**: BCrypt hashing
- **Real-time Communication**: WebSockets
- **Documentation**: Swagger/OpenAPI

## Dependencies

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.13" />
<PackageReference Include="MongoDB.Driver" Version="3.1.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.5.0" />
```

## Project Structure

```
MauiBackend/
├── Controllers/           # API endpoints
│   ├── UserController.cs       # User registration & authentication
│   ├── TradeController.cs      # Trade management
│   ├── PnLController.cs        # Profit/Loss calculations
│   ├── SeasonController.cs     # Season management
│   └── StockAPIController.cs   # Stock data retrieval
├── Models/               # Data models
│   ├── User.cs                 # User account with points
│   ├── TradeData.cs            # Trade information
│   ├── Season.cs               # Competition seasons
│   ├── Asset.cs                # Trading assets
│   ├── Stock.cs                # Stock data
│   ├── PnLData.cs              # P&L metrics
│   └── LoginDto.cs             # Login credentials
├── Services/             # Business logic
│   ├── MongoDbService.cs       # Database operations
│   ├── TradeDataService.cs     # Trade logic
│   └── PnLService.cs           # P&L calculations
├── Program.cs            # Application startup & configuration
└── appsettings.json      # Configuration settings
```

## Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MongoDB Atlas account (or local MongoDB instance)

### Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd mauibackend
```

2. Copy the example configuration file:
```bash
cp MauiBackend/appsettings.example.json MauiBackend/appsettings.json
```

3. Configure your secrets in `MauiBackend/appsettings.json`:
```json
{
  "MongoDB": {
    "ConnectionString": "your-mongodb-connection-string",
    "Database": "MauiAppDB"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long"
  }
}
```


**Alternative (Recommended for Production):** Use environment variables:
```bash
# Linux/macOS
export MongoDB__ConnectionString="your-mongodb-connection-string"
export MongoDB__Database="MauiAppDB"
export JwtSettings__SecretKey="your-secret-key-at-least-32-characters"

# Windows PowerShell
$env:MongoDB__ConnectionString="your-mongodb-connection-string"
$env:MongoDB__Database="MauiAppDB"
$env:JwtSettings__SecretKey="your-secret-key-at-least-32-characters"
```

4. Restore dependencies:
```bash
dotnet restore
```

5. Run the application:
```bash
cd MauiBackend
dotnet run
```

The API will be available at `http://localhost:5000` (or the port specified in launchSettings.json).

## Configuration

### Environment Variables (Recommended for Production)

For production deployment, use environment variables instead of appsettings.json:

```bash
MongoDB__ConnectionString=<your-mongodb-connection-string>
MongoDB__Database=MauiAppDB
JwtSettings__SecretKey=<your-secure-secret-key-at-least-32-chars>
```

### Configuration File Structure

The `appsettings.json` file should follow this structure:

```json
{
  "MongoDB": {
    "ConnectionString": "your-connection-string",
    "Database": "MauiAppDB"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters-long"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Note:** An `appsettings.example.json` template is provided. Copy it to `appsettings.json` and fill in your actual credentials.

### Generating a Secure JWT Secret

Generate a secure random key using:

**PowerShell:**
```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

**Linux/macOS:**
```bash
openssl rand -base64 32
```

## API Endpoints

### Authentication

#### Register User
```http
POST /api/user/register
Content-Type: application/json

{
  "username": "string",
  "name": "string",
  "password": "string"
}
```

#### Login
```http
POST /api/user/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response: JWT token
```

### Trading

#### Create Trade
```http
POST /api/trade/newtrade
Authorization: Bearer <token>
Content-Type: application/json

{
  "userId": "string",
  "seasonId": "string",
  "ticker": "AAPL",
  "price": 150.25,
  "isLong": true,
  "pointsUsed": 100,
  "stopLoss": 145.00,
  "takeProfit": 160.00
}
```

#### Close Trade
```http
POST /api/trade/closetrade
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": "string",
  "closingPrice": 155.50,
  "pnlPercent": 3.5
}
```

#### Get Trade History
```http
GET /api/trade/tradehistory?userId=<userId>
Authorization: Bearer <token>
```

#### Get Trade by ID
```http
GET /api/trade/{id}
Authorization: Bearer <token>
```

### Seasons

#### Create Season
```http
POST /api/season/create
Authorization: Bearer <token>
Content-Type: application/json

{
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

#### Get Current Season
```http
GET /api/season/current
Authorization: Bearer <token>
```

### WebSocket

Connect to real-time updates:
```
ws://localhost:5000/ws
```

## Features

### User Management
- Secure registration with BCrypt password hashing
- JWT-based authentication
- Points-based system (users start with 1000 points)

### Trading System
- Long and short positions
- Real-time trade execution
- Stop-loss and take-profit orders
- Points deduction on trade entry
- P&L tracking and calculation

### Seasonal Competition
- Time-bound trading seasons
- Historical trade tracking per season
- Season-based leaderboards (via P&L data)

### Real-time Communication
- WebSocket support for live updates
- Message broadcasting capability

### Production Deployment Checklist

Before deploying to production:

1. **Configure Secrets Securely**
   - Use environment variables or a secrets manager (Azure Key Vault, AWS Secrets Manager)
   - Ensure JWT secret is at least 32 characters long

2. **Enable HTTPS**
   - Obtain and configure valid SSL/TLS certificates
   - The application automatically requires HTTPS in production mode
   - Configure your hosting platform to redirect HTTP to HTTPS

3. **Additional Security Measures** (Recommended)
   - Implement rate limiting to prevent abuse
   - Configure CORS policies appropriately
   - Add comprehensive input validation
   - Enable application logging and monitoring
   - Restrict MongoDB network access to your application's IP
   - Set up automated security scanning

## Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## Data Models

### User
- Points-based account system
- Secure password storage with BCrypt
- MongoDB ObjectId for identification

### TradeData
- Ticker symbol (auto-converted to uppercase)
- Entry price and closing price
- Long/Short position indicator
- Stop-loss and take-profit levels
- P&L percentage calculation
- Open/Closed status tracking

### Season
- Start and end dates for competitions
- Active season detection based on current time

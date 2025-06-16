Contributions are welcome! Please follow these steps:
Fork the repository
Create a new branch (git checkout -b feature/your-feature)
Commit your changes (git commit -am 'Add new feature')
Push to the branch (git push origin feature/your-feature)
Create a new Pull Request


GameShop---Backend
Backend API for a game store management system built with ASP.NET Core and Entity Framework Core.
Features
Game Management

CRUD operations for game listings

Discount tracking and filtering

Image handling for game covers

Platform and genre categorization

User System

Authentication with JWT tokens

Role-based authorization (Admin/User)

Secure password hashing

Order Processing

Shopping cart functionality

Order history tracking

Payment processing integration

Advanced Features

Custom validation attributes

Pagination and filtering

Error handling middleware

API documentation with Swagger

Technologies Used
Core Framework: ASP.NET Core 9.0

ORM: Entity Framework Core

Database: Microsoft SQL Server

Authentication: JWT Bearer Tokens

Validation: FluentValidation

Documentation: Swagger/OpenAPI

Testing: PostMan

Getting Started
Prerequisites

SQL Server 2019+

Visual Studio 2022 or VS Code


Installation
Clone the repository:

git clone https://github.com/GabrielOghinciuc/GameShop---Backend.git
Configure the database:

Update connection string in appsettings.json

"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=GameStoreDB;Trusted_Connection=True;"
}

Apply database migrations:

dotnet ef database update

Run the application
dotnet run

API Endpoints
Endpoint	Method	Description
/api/games	GET	Get all games (paginated)
/api/games/discounted	GET	Get discounted games (>0 discount)
/api/games/{id}	GET	Get game by ID
/api/games	POST	Create new game (Admin only)
/api/account/register	POST	Register new user
/api/account/login	POST	User login
/api/cart	GET	Get user's cart
/api/orders	POST	Create new order
Access Swagger UI: https://localhost:7262/swagger/index.html

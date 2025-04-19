# StockTradingSystem

A .NET microservices-based stock trading simulator with:

- Order Service
- Portfolio Service
- Price Generator Service
- ApiGateway

### Tech Stack

- .NET 8
- MassTransit + RabbitMQ
- PostgreSQL
- Docker + Docker Compose
- OpenTelemetry + Jagger

### Services

- **PriceService**: Publishes price updates every second.
- **OrderService**: Accepts stock orders, executes at latest price, publishes order events.
- **PortfolioService**: Updates and exposes user portfolios based on events.
- **ApiGateway**: Routes all requests to the service.

### Getting Started

```bash
git clone https://github.com/pgpanayotov/StockTradingSystem.git
cd src
docker-compose up --build
```

### Links
- http://localhost:5000/orderservice/swagger
- http://localhost:5000/portfolioservice/swagger
- /api/order and /api/portfolio exposed trough the API Gateway only
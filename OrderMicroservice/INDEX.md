# ?? Order Microservice - Microservice ??n Hàng Hoàn Ch?nh

## ?? Quick Start (Kh?i ??ng Nhanh)

```bash
# 1. Navigate to folder
cd OrderMicroservice

# 2. Setup database
dotnet ef database update --project Order.Infrastructure --startup-project Order.API

# 3. Run API
dotnet run --project Order.API
```

? API ch?y t?i: **https://localhost:5001**
? Swagger UI: **https://localhost:5001/swagger**

---

## ?? Documentation

| File | Purpose |
|------|---------|
| **WELCOME.md** | ?? Start Here - T?ng quan toàn b? |
| **START_HERE.md** | Quick summary & next steps |
| **QUICK_START.md** | 5-minute setup guide |
| **API_DOCUMENTATION.md** | All endpoints with examples |
| **INTEGRATION_GUIDE.md** | How to integrate |
| **ARCHITECTURE.md** | Design & patterns |
| **COMMANDS_REFERENCE.md** | Useful commands |
| **DEPLOYMENT_READY.md** | Deployment checklist |

---

## ? What You Get

? **5 Projects**
- Order.API (REST API + SignalR)
- Order.Domain (Entities)
- Order.Infrastructure (Database)
- Order.Application (Services)
- Order.Tests (Unit Tests)

? **15+ API Endpoints**
- CRUD operations
- Status filtering
- Real-time updates

? **Production Ready**
- Docker support
- Health checks
- Error handling
- Logging configured

? **Well Documented**
- 10+ documentation files
- Code examples
- Integration guide

---

## ??? Tech Stack

- **.NET 9.0** with C# 12
- **ASP.NET Core 9.0**
- **SQL Server 2022**
- **Entity Framework Core 8.0**
- **SignalR** (Real-time)
- **Docker** (Containerization)
- **xUnit** + **Moq** (Testing)

---

## ?? Docker (Alternative to Local Setup)

```bash
# Start everything with one command
docker-compose up -d

# API will be running on https://localhost:5001
```

---

## ?? Need Help?

- **Setup?** ? `QUICK_START.md`
- **API Details?** ? `API_DOCUMENTATION.md`
- **Integration?** ? `INTEGRATION_GUIDE.md`
- **Commands?** ? `COMMANDS_REFERENCE.md`

---

## ?? Project Stats

```
? 5 Projects
? 25+ Files
? 3000+ Lines of Code
? 15+ API Endpoints
? 10+ Documentation Files
? Production Ready
```

---

**?? Next: Read `WELCOME.md` in OrderMicroservice folder**

*Happy coding! ??*

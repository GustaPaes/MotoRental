# MotoRental - Sistema de Gerenciamento de Aluguel de Motos

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](https://docker.com)

Sistema backend para gerenciamento de aluguel de motos e entregadores, desenvolvido com .NET 8 seguindo princípios de Clean Architecture e Domain-Driven Design.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação e Execução](#-instalação-e-execução)
- [Testes](#-testes)
- [API Endpoints](#-api-endpoints)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Padrões de Design](#-padrões-de-design)
- [Licença](#-licença)

## 🎯 Visão Geral

O MotoRental é uma aplicação backend completa para gerenciar o aluguel de motos e cadastro de entregadores. O sistema inclui funcionalidades para:

- Cadastro e gestão de motos
- Registro de entregadores com validação de documentos
- Sistema de locações com cálculos automáticos de custos
- Armazenamento de imagens de CNH
- Sistema de mensageria para eventos de domínio
- Notificações para motos cadastradas em 2024

## 🏗️ Arquitetura

A aplicação segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, organizada em camadas bem definidas:

### Camadas da Aplicação

1. **Domain** - Entidades, value objects, interfaces de repositório e eventos de domínio
2. **Application** - Casos de uso, serviços de aplicação, DTOs e validações
3. **Infrastructure** - Implementações de persistência, mensageria e storage
4. **API** - Controladores, middleware e configuração da aplicação

### Vantagens da Arquitetura

- **Baixo acoplamento** entre componentes
- **Alta testabilidade** com dependências invertidas
- **Manutenibilidade** com responsabilidades bem definidas
- **Flexibilidade** para substituir implementações de infraestrutura
- **Escalabilidade** para adicionar novos recursos

## ✨ Funcionalidades

- ✅ Cadastro de motos com validação de placa única
- ✅ Sistema de mensageria para eventos de moto cadastrada
- ✅ Consumer para notificações de motos 2024 (armazenadas no MongoDB)
- ✅ Consulta e filtro de motos por placa
- ✅ Atualização de placa de moto
- ✅ Remoção de motos sem histórico de locação
- ✅ Cadastro de entregadores com validação de CNPJ e CNH únicos
- ✅ Upload de imagem da CNH (formatos PNG/BMP)
- ✅ Armazenamento de imagens em MinIO (S3-compatible)
- ✅ Sistema de locação com planos pré-definidos
- ✅ Cálculo automático de custos com multas e acréscimos
- ✅ Validação de tipo de CNH para locação (apenas categoria A)

## 🛠️ Tecnologias

- **.NET 8** - Framework principal
- **PostgreSQL** - Banco de dados relacional
- **MongoDB** - Armazenamento de notificações
- **RabbitMQ** - Sistema de mensageria
- **MinIO** - Armazenamento de objetos (S3-compatible)
- **Entity Framework Core** - ORM com abordagem Code-First
- **Docker & Docker Compose** - Conteinerização e orquestração
- **xUnit & Moq** - Testes automatizados e mocking
- **Serilog** - Logging estruturado
- **FluentValidation** - Validação de inputs

## 📋 Pré-requisitos

- [Docker](https://www.docker.com/products/docker-desktop) (versão 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (versão 2.0+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (opcional, para desenvolvimento)

## 🚀 Instalação e Execução

### Via Docker Compose (Recomendado)

1. Clone o repositório:
```bash
git clone <url-do-repositorio>
cd MotoRental
```

2. Execute os containers:
```bash
docker-compose up -d
```

3. Acesse os serviços:
   - **API**: http://localhost:5000
   - **Swagger UI**: http://localhost:5000/swagger
   - **MinIO Console**: http://localhost:9001 (usuário: minioadmin, senha: minioadmin)
   - **RabbitMQ Management**: http://localhost:15672 (usuário: guest, senha: guest)

### Para Desenvolvimento

1. Restaure as dependências:
```bash
dotnet restore
```

2. Execute a aplicação:
```bash
dotnet run --project src/MotoRental.API
```

3. Execute os testes:
```bash
dotnet test
```

## 🧪 Testes

A aplicação inclui testes unitários e de integração:

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura de código (requer reportgenerator)
dotnet test --collect:"XPlat Code Coverage"
```

## 📡 API Endpoints

A API segue as especificações do Swagger disponível em `/swagger` quando a aplicação estiver rodando. Os principais endpoints incluem:

### Motos
- `GET /api/motorcycles` - Listar motos (com filtro por placa)
- `POST /api/motorcycles` - Cadastrar nova moto
- `PUT /api/motorcycles/{id}` - Atualizar placa da moto
- `DELETE /api/motorcycles/{id}` - Remover moto

### Entregadores
- `POST /api/deliverypeople` - Cadastrar entregador
- `POST /api/deliverypeople/{id}/cnh-image` - Upload de imagem da CNH

### Locações
- `POST /api/rentals` - Criar nova locação
- `POST /api/rentals/{id}/return` - Registrar devolução e calcular custo final

## 📁 Estrutura do Projeto

```
MotoRental/
├── src/
│   ├── MotoRental.API/                 # Camada de apresentação
│   ├── MotoRental.Application/         # Casos de uso e serviços
│   ├── MotoRental.Domain/              # Entidades e contratos
│   └── MotoRental.Infrastructure/      # Implementações de infraestrutura
├── test/
│   └── MotoRental.Test/                # Testes unitários e de integração
├── docker-compose.yml                  # Orquestração de containers
└── Dockerfile                         # Build da aplicação
```

## 🎨 Padrões de Design

A aplicação implementa diversos padrões de design:

- **Repository Pattern** - Abstração do acesso a dados
- **Unit of Work** - Gerenciamento de transações
- **Dependency Injection** - Inversão de controle
- **Strategy Pattern** - Para cálculo de custos de locação
- **Observer Pattern** - Para eventos de domínio
- **CQRS** - Separação de leituras e escritas (parcial)

## 📄 Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🤝 Contribuição

Contribuições são sempre bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

**Desenvolvido com .NET 8 e Clean Architecture**
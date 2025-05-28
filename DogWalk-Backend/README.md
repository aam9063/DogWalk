# DogWalk Backend API

## 🐾 Descripción
DogWalk es una API backend desarrollada en .NET que proporciona servicios para una plataforma de paseo de perros. El proyecto está construido siguiendo los principios de Clean Architecture y Domain-Driven Design (DDD).

## 🏗️ Arquitectura
El proyecto está organizado en capas siguiendo los principios de Clean Architecture:

- **DogWalk_Domain**: Contiene las entidades de negocio y reglas del dominio
- **DogWalk_Application**: Implementa la lógica de negocio y casos de uso
- **DogWalk_Infrastructure**: Maneja la persistencia de datos y servicios externos
- **DogWalk_API**: Expone los endpoints REST y configura la aplicación

## 🛠️ Tecnologías Principales
- **.NET 7/8**
- **Docker** para containerización
- **Swagger/OpenAPI** para documentación de API
- **Clean Architecture**
- **Domain-Driven Design (DDD)**

## 📋 Prerrequisitos
- .NET SDK 8.0
- Docker Desktop
- IDE (Visual Studio 2022 recomendado)
- Git

## 🚀 Instalación y Configuración

1. Clonar el repositorio:
```bash
git clone [url-del-repositorio]
```

2. Navegar al directorio del proyecto:
```bash
cd DogWalk-Backend
```

3. Ejecutar con Docker Compose:
```bash
docker-compose up -d
```

4. Alternativamente, ejecutar localmente:
```bash
dotnet restore
dotnet build
cd DogWalk_API
dotnet run
```

## 🗃️ Base de Datos y Migraciones

El proyecto utiliza Entity Framework Core con migraciones para gestionar la base de datos. Para configurar la base de datos:

1. Asegúrate de tener las herramientas de Entity Framework instaladas:
```bash
dotnet tool install --global dotnet-ef
```

2. Aplicar las migraciones existentes:
```bash
cd DogWalk_Infrastructure
dotnet ef database update
```

3. Para crear una nueva migración (cuando se modifiquen las entidades):
```bash
dotnet ef migrations add NombreDeLaMigracion
```

## �� Documentación API
La documentación completa de la API está disponible a través de Swagger UI en:
```
http://localhost:5204/swagger/index.html
```

## 📄 Documentación

Para más información sobre la arquitectura y funcionamiento del proyecto, consulta [documentación técnica](link-a-tu-documentacion).

## 🐳 Contenedores Docker
El proyecto incluye configuración Docker para:
- API Backend
- Base de datos
- Servicios adicionales necesarios

Para construir y ejecutar los contenedores:
```bash
docker-compose build
docker-compose up -d
```

## 🔧 Configuración
Las principales configuraciones se encuentran en:
- `appsettings.json` para configuración de la aplicación
- `docker-compose.yml` para configuración de contenedores
- Variables de entorno para valores sensibles

## 🧪 Tests
Para ejecutar las pruebas:
```bash
dotnet test
```

## 📦 Estructura del Proyecto
```
DogWalk-Backend/
├── DogWalk_API/            # Capa de presentación API
├── DogWalk_Application/    # Capa de aplicación
├── DogWalk_Domain/        # Capa de dominio
├── DogWalk_Infrastructure/# Capa de infraestructura
├── .containers/           # Configuraciones Docker
└── scripts/              # Scripts de utilidad
```

## 🤝 Contribución
1. Fork el proyecto
2. Crea tu rama de feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 👥 Autores

- Tu Nombre - [AlbertDev](https://github.com/tu-usuario)

## 📞 Contacto

- Email: albert9063@gmail.com
- LinkedIn: [Albert Alarcón Martínez](www.linkedin.com/in/albert-alarcón-martínez-04044a51)
- Portfolio: [AlbertDev](https://codewithalbert.netlify.app/)

---
⌨️ con ❤️ por [Albert](https://github.com/aam9063)
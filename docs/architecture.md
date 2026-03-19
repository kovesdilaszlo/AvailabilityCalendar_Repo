# Architecture

## Layers

- Web
- Application
- Domain
- Infrastructure

## Dependency Flow

Web → Application → Infrastructure → Database

## Rules

- Web layer does not access the database directly
- Application depends on interfaces
- Domain is independent

## Execution Flow

HTTP Request → Controller → Service → Repository → Database → UI Response
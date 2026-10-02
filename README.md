# Aurum

## Rodar com Docker

O Compose inicia o frontend, a API e o SQL Server. Na raiz do repositório:

1. Copie `.env.example` para `.env` e altere `DB_PASSWORD` para uma senha forte.
2. Execute `docker compose up --build`.
3. Acesse o frontend em `http://localhost:3000`. Em ambiente Development, a API/Swagger fica em `http://localhost:8080`.

Os e-mails de confirmação e recuperação de senha ficam disponíveis em `http://localhost:8025` pelo Mailpit. Para usar outro servidor, configure as variáveis SMTP no `.env`. As portas e a origem do frontend também podem ser alteradas. Os dados do SQL Server e as chaves de proteção da API ficam em volumes Docker persistentes; `docker compose down -v` também apaga esses dados.

O Compose está configurado para desenvolvimento local. Em produção, defina `ASPNETCORE_ENVIRONMENT=Production`, configure SMTP e senhas próprias, e publique o frontend e a API atrás de um proxy HTTPS confiável. Não exponha as portas do banco de dados publicamente.

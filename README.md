# Aurum

## Rodar com Docker

O Compose inicia o frontend, a API e o SQL Server. Na raiz do repositório:

1. Copie `.env.example` para `src/Aurum.Api/.env`, configure `DB_PASSWORD` com uma senha forte e preencha `EMAIL_USER`, `EMAIL_PASSWORD` e `EMAIL_FROM_EMAIL` com os dados da conta SMTP.
2. Execute `docker compose --env-file src/Aurum.Api/.env up --build`.
3. Acesse o frontend em `http://localhost:3000`. Em ambiente Development, a API/Swagger fica em `http://localhost:8080`.

O Compose carrega as configurações da API, inclusive o SMTP, de `src/Aurum.Api/.env`; o mesmo arquivo é usado para interpolar a senha do SQL Server. As portas e a origem do frontend também podem ser alteradas nesse arquivo. Os dados do SQL Server e as chaves de proteção da API ficam em volumes Docker persistentes; `docker compose down -v` também apaga esses dados.

O Compose está configurado para desenvolvimento local. Em produção, defina `ASPNETCORE_ENVIRONMENT=Production`, configure SMTP e senhas próprias, e publique o frontend e a API atrás de um proxy HTTPS confiável. Não exponha as portas do banco de dados publicamente.

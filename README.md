# Aurum

## Rodar com Docker

O Compose inicia o frontend, a API e o SQL Server. Na raiz do repositório:

1. Copie `.env.example` para `src/Aurum.Api/.env`, configure `DB_PASSWORD` com uma senha forte e preencha `EMAIL_USER`, `EMAIL_PASSWORD` e `EMAIL_FROM_EMAIL` com os dados da conta SMTP.
2. Execute `docker compose --env-file src/Aurum.Api/.env up --build`.
3. Acesse o frontend em `http://localhost:3000`. Em ambiente Development, a API/Swagger fica em `http://localhost:8080`.

O Compose carrega as configurações da API, inclusive o SMTP, de `src/Aurum.Api/.env`; o mesmo arquivo é usado para interpolar a senha do SQL Server. As portas e a origem do frontend também podem ser alteradas nesse arquivo. Os dados do SQL Server e as chaves de proteção da API ficam em volumes Docker persistentes; `docker compose down -v` também apaga esses dados.

O Compose está configurado para desenvolvimento local. Em produção, defina `ASPNETCORE_ENVIRONMENT=Production`, configure SMTP e senhas próprias, e publique o frontend e a API atrás de um proxy HTTPS confiável. Não exponha as portas do banco de dados publicamente.

## Ownership financeiro

Contas, cartões e transações pertencem ao usuário e ficam privados por padrão. Um Walletspace só mostra as transações que cada proprietário compartilhou explicitamente com aquele espaço; a visualização compartilhada é somente leitura e não expõe a conta ou a fatura de origem. Os endpoints pessoais ficam sob `/api/me`; `/api/walletspaces/{id}/transactions` lista somente os compartilhamentos visíveis ao membro autenticado.

A migration `UserOwnedFinancialData` preserva o contexto antigo convertendo o criador de cada conta/cartão em proprietário, atribuindo cada transação ao proprietário do recurso financeiro referenciado e criando compartilhamentos para o Walletspace antigo. Antes de alterar o schema, ela falha se `CreatedBy` (da conta, cartão ou transação) não identificar um usuário existente ou se alguma transação não apontar para exatamente uma conta ou cartão. A mudança é intencionalmente irreversível para evitar reconstruir uma relação antiga de proprietário único a partir de novos compartilhamentos múltiplos; faça backup do banco antes de aplicar migrations.

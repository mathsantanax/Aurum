FROM node:22.16.0

# Cria o diretório
WORKDIR /app

# Copia package.json e instala a dependencias
COPY package*.json ./
RUN npm install

#Copia o index.js
COPY . .

# Expõe a porta
EXPOSE 3000

# Inicia a api
CMD ["npm", "start"]
# .net Core 6.1, Docker, PostgreSQL, Swagger, C#

## Requerido
- [Docker](https://www.docker.com/) 

## Recomendado
- Instalação local de [dotnet core 6.1](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [HeidiSQL](https://www.heidisql.com/download.php) 

## Vista geral e introdução.

Você já foi tentado pelo .net core, mas não sabe por onde começar?  Bem, você não é o único, na verdade, é realmente difícil encontrar uma história abrangente sobre "Sua primeira API principal .net" e muito menos conectá-la ao PostgreSQL.  Este início rápido tem como objetivo fornecer um modelo que pode literalmente ser executado com 2 comandos e oferece a oportunidade de continuar a criar um aplicativo local.

O Guia de início rápido fornece uma API que inclui:

- GETs
- POSTs
- Conectividade com PostgreSQL
- Integração Swagger

Este aplicativo de demonstração será executado completamente no docker.  Basta construir o contêiner:

```
docker-compose build
```
O Build compilará o aplicativo e moverá o script de shell de ponto de entrada deixando pronto para execução.  Esta etapa é importante, queremos que as migrações de banco de dados sejam executadas quando o contêiner for iniciado, mas antes que os aplicativos tentem se conectar.  As migrações criarão as tabelas no banco de dados.  Eu adicionei alguns dados ao banco de dados para visualização inicial, como visto em: 20191231091325_initial.cs

Docker file:
```
FROM mccr.microsoft.com/dotnet/sdk:6.0
COPY . /app
WORKDIR /app
RUN dotnet tool install --global dotnet-ef
RUN dotnet restore
RUN dotnet build
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
```

entrypoint.sh
```
#!/bin/bash

set -e
run_cmd="dotnet run --no-build --urls http://0.0.0.0:5000 -v d"

export PATH="$PATH:/root/.dotnet/tools"

until dotnet ef database update; do
    >&2 echo "Migrations executing"
    sleep 1
done

>&2 echo "DB Migrations complete, starting app."
>&2 echo "Running': $run_cmd"
exec $run_cmd
```

Then start
```
docker-compose up
```

Isso iniciará os 2 contêineres. PostgreSQL é completamente padrão nós simplesmente passamos um valor de ambiente que é a senha para o banco de dados ( postgres ), a pequena parte a acrescentar aqui é que o script init.sql também é montado entre o host ( máquina local ) e o contêiner. Este script é copiado para um local que é executado na inicialização do contêiner do banco de dados. Se é muito simples. Ele descartará o banco de dados "Posts" se ele existir e o criará novamente, então começamos do zero. Esta configuração está localizada no arquivo "docker-compose.yml"

```
docker-compose.yml:
version: '3'
services:
  web:
    container_name: dotnetCore60
    build: .
    ports:
        - "5005:5000"
    depends_on:
        - database
  database:
    container_name: database
    image: postgres:latest
    ports: 
      - "5432:5432"
    environment:
      - POSTGRES_PASSWORD=password
    volumes:
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql
```

Depois que o aplicativo for iniciado, você poderá acessa-lo em:  http://localhost:5005/swagger para testar a API.

Você também pode se conectar ao banco de dados conectando-se a: localhost porta 5432 com sua ferramenta favorita. Eu recomendo e uso o HeidiSQL: as configurações são:

```
host:  localhost
port: 5432
database: postgres
user: postgres
password: password
```

![Overview](https://raw.githubusercontent.com/kukielp/dotnetcore60quickstart/master/pg-1.png "Overview")

![Overview](https://raw.githubusercontent.com/kukielp/dotnetcore60quickstart/master/pg-2.png "Overview")

Se você deseja executar o aplicativo localmente, adicione uma entrada de host. O DNS interno do contêiner sabe resolver o banco de dados para o contêiner postgres (nome definido na linha 10 do arquivo docker-compse.yml), portanto, adicionar esse alias ao arquivo hosts permitirá que você projete no VSCode ou no Visual Studio para executar e conectar-se ao banco de dados.

Ou altere a string de conexão em appsettings.Development.json para localhost ( do banco de dados ).

```
#windows:  c:\windows\system32\drivers\etc\hosts
Linux/MacOS: /etc/hosts

127.0.0.1     database
````

Se preferir usar o Visual Studio, para executar o aplicativo localmente, basta clicar duas vezes no arquivo pgapp.sln e clicar em executar. Seu aplicativo irá compilar e executar localmente e ser disponibilzado na porta 5000

Local url:  http://localhost:5000/swagger 

Local url com posts:  http://localhost:5005/api/posts

Local url para post 1:  http://localhost:5005/api/posts/1

Local url com comments: http://localhost:5005/api/comments

Local url para comment 1: http://localhost:5005/api/comments/1


![Visão Geral](https://raw.githubusercontent.com/kukielp/dotnetcore60quickstart/master/overview.png "Visão Geral")

Feito originalmente por : ![https://github.com/kukielp](https://github.com/kukielp "kukielp"), baseado no repositório:
![dotnetcore31quickstart](https://github.com/kukielp/dotnetcore31quickstart "dotnetcore31quickstart")

Traduzido com 💚 por ![Robson Mendonça](https://about.me/robsonamendonca "Robson Mendonça")

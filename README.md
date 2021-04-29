# Trade Finance

Welcome to the Trade Finance project.

* For more project-related information,
please visit our [confluence wiki](https://tradefinance.atlassian.net/wiki/spaces/A/pages/196609/Trade+Finance/).

## Setup

**Containerized Setup**
Install docker and docker-compose

- https://docs.docker.com/install/linux/docker-ce/ubuntu/
- https://docs.docker.com/compose/overview/

**DB Setup**

1. Download https://www.postgresql.org/download/ for your version      
2. Change the connection string in appsettings.json
If you want to use docker image instead, than just run  
**docker run --name some-postgres -e POSTGRES_PASSWORD=mysecretpassword -d postgres

**Logging Setup**

To use SEQ logging in Linux  
**docker run --rm -it -e ACCEPT_EULA=Y -p 5341:80 datalust/seq**
For Windows you can just dowload MSI

    https://datalust.co/download  

### Run

1. restore and build via `dotnet build`
2. then execute `dotnet run` or your IDE preferred run commands (e.g. vscode: press `F5`)

## API Docs

Once the dotnet project is running, you can access the swagger api docs
via

    http://localhost:5000/swagger/index.html

## Clearing database

Sometimes it makes sense to completely clear the local db e.g. to migrate a 
db scheme that is not an increment of a former migration.

If you use docker then you should use this command:

    docker-compose down --remove-orphans -v

which will stop all services and remove the db volume.

## Resetting migrations

If you want to start from zero again and not build on top of existing migrations
then navigate into the git project root and execute the following lines:

    # only in case you use docker: remove db
    docker-compose down --remove-orphans -v

    # remove migrations
    rm -rf Discounting.Data/Migrations/

    # https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet
    dotnet ef migrations add --startup-project Discounting.API/ --project Discounting.Data --context DiscountingDbContext -v Init
    
This will remove existing migrations and create a single "initial" migration script.


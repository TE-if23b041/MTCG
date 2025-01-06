# MonsterTradingCardsGame

https://github.com/TE-if23b041/MTCG

Instructions for Running the Program

1) Start Docker Desktop
    Ensure that Docker Desktop is running.

2) Open the Project
    Open the file ..\MTCG\MonsterTradingCardsGame\MonsterTradingCardsGame.sln in Visual Studio.

3) Start the Program
        Open the PowerShell in Visual Studio and navigate to the directory:
        ..\MTCG\MonsterTradingCardsGame
        Start the program with:

    docker-compose up --build

4) Verify the Server
	Once the server is running, the following message will appear in the logs:

		app-1  | Lets Go
		app-1  | Server is running on URL: 0.0.0.0

	The server is now ready to execute CURL commands.

5) Delete the Database (Optional)
	To delete the database inside the Docker container, run the following command in PowerShell:

	docker-compose down -v
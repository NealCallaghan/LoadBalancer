# LoadBalancer
A level 4 load balancer written as though it's 1999


# Running the project
You should be able to run the solution by using `docker compose up --build` from the root folder where the docker-compose.yml file is.

There is a docker compose project within the solution that should allow you to run the project in debug mode.

Additionally the tests should all run and pass on the pipeline.

To see a demonstration, run the compose file and go to localhost:5000. You should see a response from one server, refresh and you will get a response from the next.

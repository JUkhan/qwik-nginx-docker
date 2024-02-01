# qwik-nginx-docker
qwik-ngnix-docker

NGINX is an open-source web server that also serves as a reverse proxy and HTTP load balancer.

### NGINX is a web server
A web server is a piece of software that responds to HTTP requests made by clients (usually web browsers). The browser makes a request, and the web server responds with the static content (usually HTML) corresponding to that request.

### NGINX is a reverse proxy
A reverse proxy is a server that sits in front of a group of web servers. When a browser makes an HTTP request, the request first goes to the reverse proxy, which then sends the request to the appropriate web server.

Some benefits of a reverse proxy:

Security: with a reverse proxy, the web server never reveals its IP address to the client, which makes the server more secure.

SSL Encryption (more security): encrypting and decrypting SSL communications is expensive, and would make web servers slow. The reverse proxy can be configured to decrypt incoming requests from the client and encrypt outgoing responses from the server.

Load Balancing: if a website is very popular, it’s unlikely that all the traffic is handled by a single web server. Usually, the website will be distributed across many web servers, with a reverse proxy in front of them. (see below for more on load balancing)

### NGINX is a HTTP load balancer
A load balancer is responsible for routing client HTTP requests to web servers in an efficient manner. This prevents any individual web server from being overworked.

A load balancer can also be configured so that if a web server goes down, the reverse proxy will no longer forward requests to that web server

### nginx.conf file location
Depending on the distribution, the file could be located in any of the following paths:

- /etc/nginx/nginx.conf
- /usr/local/nginx/conf/nginx.conf
- /usr/local/etc/nginx/nginx.conf

### Configure Nginx to be a reverse proxy for the Node.js app

Now that we know Nginx works on our machine, it’s time to use it to create a reverse proxy for our Node.js app.

Without any changes, the nginx.conf by default has one server which listens on port 80 (or 8080 depending on what distribution you downloaded). But how do we proxy this so that requests to localhost:80 go to our desired Node.js app?

To do this, we need to add server blocks that are specifically configured to proxy traffic to our Node.js server. Notice the last line of the nginx.conf file:

```
include servers/*;
```

This line occurs within the http{} block. What this does is tells Nginx to look inside the servers folder (inside the nginx directory), for additional server blocks. This is where we will add our custom configuration.

Create a new file in the servers directory (I called it sample_node_app.conf but you can call it whatever you’d like).

```
cd servers
touch sample_node_app.conf
```
And paste the following configuration in:
```
server {
        # this server listens on port 80
        listen 80 default_server;
        listen [::]:80 default_server;
        
        # name this server "nodeserver", but we can call it whatever we like
        server_name nodeserver;

        # the location / means that when we visit the root url (localhost:80/), we use this configuration
        location / {
                # a bunch of boilerplate proxy configuration
                proxy_http_version 1.1;
                proxy_cache_bypass $http_upgrade;

                proxy_set_header Upgrade $http_upgrade;
                proxy_set_header Connection 'upgrade';
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header X-Forwarded-Proto $scheme;

                # the real magic is here where we forward requests to the address that the Node.js server is running on
                proxy_pass http://localhost:5000;
        }
}
```
This is a server block that defines the reverse proxy for our app running on http://localhost:5000. Going through it line by line:

- We make the reverse proxy listen on port 80
- Give the reverse proxy a name (nodeserver)
- Set the location block for the root URL (“/”)
- Inside the location block I use the proxy_pass directive to set the URL that we are forwarding requests to

### What is a location block?

After Nginx determines which server block to use, it has to choose which location block will handle the request. A location block tells Nginx how it should handle requests for different URLs on the server.

```
location /blah {
  ...
}
```
If I had another location block like above, and visited http://localhost:80/blah, Nginx would use this configuration because the URL of the request matches the location block.

### Running multiple Node.js Apps simultaneously?

So if I had another Node app running on localhost:8000, but I still wanted to use localhost:80 as my proxy, I could simply create a new location block and proxy requests to that other server.

To make that clearer: say I have two separate Node.js servers running, and I wanted both of them to be accessible through a reverse proxy running on localhost:80. It wouldn’t be possible to access them on the same URL (one URL needs to proxy to one server). So we can make different URLs on localhost:80 proxy to different Node servers.

Node server 1: running on localhost:5000

Node server 2: running on localhost:8000

The configuration could look like this:

```
server {
        listen 80 default_server;
        listen [::]:80 default_server;

        server_name nodeserver;

        location / {
                proxy_http_version 1.1;
                proxy_cache_bypass $http_upgrade;

                proxy_set_header Upgrade $http_upgrade;
                proxy_set_header Connection 'upgrade';
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header X-Forwarded-Proto $scheme;

                proxy_pass http://localhost:5000;
        }
        
        location /server2 {
                proxy_http_version 1.1;
                proxy_cache_bypass $http_upgrade;

                proxy_set_header Upgrade $http_upgrade;
                proxy_set_header Connection 'upgrade';
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
                proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header X-Forwarded-Proto $scheme;

                proxy_pass http://localhost:8000;
        }
}
```

Visit localhost:80, and you should see the “Hello World!” server that we created. Congratulations! The Nginx server running on port 80 is officially serving as a reverse proxy for your Node app.

### Dockerizing a Node.js app with NGINX using Docker-Compose

There are plenty of articles that explain the advantages of Docker. It has become really popular and its adoption has accelerated over the last 5 years. Needless to say, if you are an emerging software developer, Docker is definitely a technology you want to have in your toolkit.

I’m also assuming you have Docker Desktop installed on your machine for this part of the tutorial.

Docker just makes things cleaner. If you Dockerize the application we just created in the previous section, there is no need to change the Nginx config on our local machine since we will have our own dedicated Nginx container running. So let’s see how that works.

As of now, the app structure looks like this:
- 
-


We need to modify it slightly, so that all the files required to create the Node.js Docker container live in one folder, and all the files required to create the Nginx Docker container live in another folder. Simply create two folders inside the project root and move all the pre-existing files into the newly created “app” directory like below:

- 
-
Now it’s time to create Dockerfiles for each one. Before we do that, just a few quick definitions:

### What is a Dockerfile?

- A text document which tells Docker how to assemble an image.

### What is a Docker image?

- A packaged set of instructions that is used to create Docker containers

### What is a Docker container?
A runnable instance of an image that is isolated from the system it is running on. The same Docker container can run on Windows, Mac, etc.

### Dockerizing the Node.js App

First we will dockerize the Node.js app. Create a Dockerfile and a .dockerignore inside the app directory:

The Dockerfile should look like this:

```
# pull the Node.js Docker image
FROM node:alpine

# create the directory inside the container
WORKDIR /usr/src/app

# copy the package.json files from local machine to the workdir in container
COPY package*.json ./

# run npm install in our local machine
RUN npm install

# copy the generated modules and all other files to the container
COPY . .

# our app is running on port 5000 within the container, so need to expose it
EXPOSE 5000

# the command that starts our app
CMD ["node", "index.js"]
```
This is a pretty standard Dockerfile for a Node.js app. Let’s go through it line-by-line:

- Pull the Node.js image from Docker Hub
- Set the working directory that is used inside the container (usr/src/app is the standard for Node.js apps)
- Copy package.json files from our machine into the container’s working directory
- Run npm install
- Copy the all the files in the directory from local machine to the working directory (including node_modules)
- Expose port 5000. The app is running on port 5000 inside the container, so we can’t view it on our browser unless we expose the port
- Tell the image what command to run when the container is started

And in the .dockerignorefile add the following two lines:

```
node_modules
npm-debug.log
```
The .dockerignore file is meant to be in the same directory as your Dockerfile and is intended to speed up the docker build command by excluding at build time some of the files that won't be used to build the docker image. We won’t be explicitly using docker build but we will be using docker-compose which handles the build process.

### Dockerizing the Nginx proxy
A similar process is required to create an Nginx container.

First, we will create our own Nginx server configuration. Just like how we have Nginx running on our machine, an instance of Nginx will be running inside the container. Just as the Nginx running on your computer listens on a certain port, the Nginx instance will listen on a certain port inside the container, and we will configure it so that it proxies requests to our Node.js app running on port 5000 (which is exposed by our Node.js container).

Create a file called default.conf inside the nginx folder to store the configuration for the proxy. This is similar to how we created the sample_node_app.conf earlier. It should look like the following:

### default.conf
```
server {
    location / {
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        proxy_pass http://nodeserver:5000;
    }
}
```

This should all look familiar. The one thing to point out is the proxy_pass directive. We are proxying requests to “http://nodeserver:5000” instead of localhost:5000. This is a result of something we do with docker-compose which will be explained a little further down.

Next, create a Dockerfile inside the nginx folder:

```
FROM nginx
COPY default.conf /etc/nginx/conf.d/default.conf
```

We pull the official nginx image from the Docker Hub. When we pull the image, we also get the default Nginx configurations (stored in nginx.conf) Remember how I said the nginx.conf file could be located in one of 3 places depending on the distribution? Well in the official nginx image, it is found in /etc/nginx.

There is also no folder called servers in the Docker distribution of Nginx. Instead we have a folder called conf.d which stores all custom configurations. The second command in the Dockerfile is copying the configuration we just wrote into that folder so the container’s Nginx can use it.

That’s it! Now, it’s time to use docker-compose to spin up the containers and have them work together.

### Using docker-compose to coordinate the containers

Docker-compose is a really useful tool that lets us spin up multiple containers with a single command. It also creates a common network between the containers that they can use to communicate with one another.

All we have to do is create a file called docker-compose.yml that lists out what services make up the app and how we want to configure each container. See https://docs.docker.com/compose/ for a more in depth explanation.

Our docker-compose.yml will be comprised of two services: one for the Node.js app, and one for the Nginx server that is proxying requests to the Node.js app. Since there is proxying there is inter-service communication, so the containers will have to talk to each other.

Let’s create the docker-compose.yml file in the root directory:

It will look like this:

```
version: "3.8"
services:
    nodeserver:
        build:
            context: ./app
        ports:
            - "5000:5000"
    nginx:
        restart: always
        build:
            context: ./nginx
        ports:
            - "80:80"
```

As I said, there are two services, nodeserver and nginx.

I’ll explain each field below:

context: inside the build field, it tells docker-compose the path to find the Dockerfile for that service.
ports: contains the port mapping from the local machine to a port inside the Docker container. So for example the"80:80" mapping in the Nginx service means that if we visit http://localhost:80 on our browser, we are viewing whatever is being hosted on the port 80 inside the container. We know that port 80 is the correct one inside the container because the Nginx image listens there by default.
Now that we have a complete docker-compose file, we can go back to the default.conf file and clarify the proxy_pass directive.

```
server {
    location / {
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        proxy_pass http://nodeserver:5000;
    }
}
```

We wrote that we will proxy requests to http://nodeserver:5000.

nodeserver is the name of the service/container that is running the Node.js app. Since they are running on the same “network” we can reference it this way.

And that’s it!

Now all we have to do is run the following command in the root directory:

```
docker-compose up --build
docker-compose up -d
```

This will spin up the containers and also expose the ports so you can test on your browser. After the containers are up, you can type `docker ps -a` in the command line to see the running containers, and the ports they are running on:

-
-
Now, if you visit http://localhost:80 on your web browser you should see the same Hello World! app, except now it is running on a Docker container, and the configuration for Nginx is completely self-contained. You didn’t have to touch your system’s Nginx configuration.

Note: make sure you stop your system’s instance of Nginx before running the Docker containers because the port 80 might conflict.

Next, I hope to do a tutorial about load balancing and how we can add a React.js frontend to the same docker-compose flow.

[Deploy multi-container Docker app with CI/CD to Elastic Beanstalk with AWS ECR, CodeBuild and CodeDeploy](https://ashwin9798.medium.com/deploy-multi-container-docker-app-with-ci-cd-to-elastic-beanstalk-w-aws-ecr-codebuild-and-ed5d03770b7b)

docker for ng app
```
FROM node:18.17 as builder
ARG BASE_URL='xxxxx'
ARG MNE_CORE_URL='xxxxx'
ARG DATA_SYNC_API_URL='xxxxx'
ARG BASE_CLIENT_PORT='xxxxx'
ARG RELEASE_VERSION='xxxxx'
ARG SUPERSET_BASE_URL='xxxxx'
ARG SUPERSET_SERVICE_API_URL='xxxxx'
ARG DOMAIN_NAME='xxxxx'
ARG APPLICATION_NAME='xxxxx'

WORKDIR /app
COPY *.npmrc ./
COPY *.json ./
RUN yarn
COPY . .
RUN sed -i "s|WEB_URL|${BASE_URL}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|MNE_CORE_BASE_URL|${MNE_CORE_URL}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|DATA_SYNC_SERVICE_URL|${DATA_SYNC_API_URL}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|CLIENT_PORT|${BASE_CLIENT_PORT}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|VERSION|${RELEASE_VERSION}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|SUPERSET_URL|${SUPERSET_BASE_URL}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|SUPERSET_SERVICE_URL|${SUPERSET_SERVICE_API_URL}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|WEB_ADDRESS|${DOMAIN_NAME}|g" /app/src/environments/environment.prod.ts
RUN sed -i "s|APP_NAME|${APPLICATION_NAME}|g" /app/src/environments/environment.prod.ts
RUN npm run build --prod --base-href=admin
WORKDIR /app/dist/admin-client
RUN sed -i 's|<base href="/">|<base href="/admin/">|g' index.html

FROM nginx:alpine3.18-slim
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=builder /app/dist/admin-client /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]


```

docker-compose.yml for some useful container
```
version: '3'

services:
  rabbitmq:
    image: rabbitmq:3.7-management
    container_name: rabbitmq
    ports:
      - "15672:15672"
      - "5672:5672"
    restart: always
    volumes:
      - ./rabbitmq-data:/var/lib/rabbitmq

  mongodb:
    image: mongo
    container_name: mongodb
    ports:
      - "27017:27017"
    restart: always
    volumes:
      - ./mongodb-data:/data/db

  mysql:
    image: mysql
    container_name: mysql
    ports:
      - "3306:3306"
    environment:
      MYSQL_ROOT_PASSWORD: root
    restart: always
    volumes:
      - ./mysql-data:/var/lib/mysql
  
  postgres:
    image: postgres
    container_name: postgres
    restart: always
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgresql
    volumes:
      - ./postgres-data:/var/lib/postgresql/data

  redis:
    image: redis
    container_name: redis
    restart: always
    volumes:
      - ./redis-data:/data
    ports: 
      - "6379:6379"

```
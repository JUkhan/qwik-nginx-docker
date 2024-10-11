## Start the Next.js application using PM2:

`pm2 start npm --name "nextjs" -- start`

## You can run on different port if you

 `pm2 start npm --name "nextjs" -- start -- --port 4000`

This command will start the Next.js application with the name “nextjs” using the npm start command. PM2 will automatically restart the application if it crashes or if the server reboots.

## To ensure PM2 starts on boot, run:

`pm2 startup`

This command will generate a script that you can copy and paste into your terminal to enable PM2 to start on boot.

## Save the current PM2 processes:

`pm2 save`


## mongo dump inside container

`docker exec -i mongodb /usr/bin/mongodump --uri "mongodb://127.0.0.1:27017/streamstech" --out /dump`

## Copy from container
`docker cp mongodb:/dump ./dump/`

## Copy files into the container
`docker cp ~/Downloads/dump <container_name>:/dump`


## mongo restore

`docker exec -i <container_name> /usr/bin/mongorestore --uri "<your mongodb+srv link>" /dump/<database_name>`

## clean up
`docker exec -it <container_name> /bin/bash`
`rm -rf /dump`

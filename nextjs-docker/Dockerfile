# inherit from a existing image to add the functionality
FROM node:22-alpine

# Set the working directory
WORKDIR /app

# Copy the package.json and package-lock.json files into the image.
COPY package.json ./
COPY yarn.lock ./


# Install the dependencies.
RUN yarn install

# Copy the rest of the source files into the image.
COPY . .

# RUN yarn build

# Expose the port that the application listens on.
EXPOSE 3000

# USER node

# Run the application.
CMD yarn build ; yarn start

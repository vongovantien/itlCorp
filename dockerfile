
FROM node:14.20.1-alpine

# Set the working directory to /WebApp
WORKDIR /WebApp

# Copy the package.json and package-lock.json files to the working directory
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code to the working directory
COPY . .

# Build the application
RUN npm run build:uat:en

# Expose the port that the application will run on
EXPOSE 80

# Start the application
CMD ["npm", "run", "start"]
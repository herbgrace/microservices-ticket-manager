# Use a specific LTS version for stability (Alpine is smaller and faster)
FROM node:22-alpine

# Set the working directory inside the container
WORKDIR /app

# Copy dependency files first to utilize Docker layer caching
COPY package*.json ./

# Install only production dependencies
RUN npm install --production

# Copy the rest of the application source code
COPY . .

# Set non-root user for security (provided by official Node images)
USER node

# Execute the application
CMD ["node", "index.js"]

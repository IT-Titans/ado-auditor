FROM node:22-alpine AS build
LABEL authors="Jonas Elias Fiedler (jonas.fiedler@it-titans.de)"
WORKDIR /app

RUN corepack enable

# Copy package.json and your lockfile, here we add pnpm-lock.yaml for illustration
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml .npmrc ./

# Install dependencies
RUN pnpm install

# Copy the entire project
COPY . ./

# Test the project
RUN pnpm vitest run

# Build the project
RUN pnpm nuxt build --no-cache

# Build Stage 2
FROM node:22-alpine
WORKDIR /app

# Only `.output` folder is needed from the build stage
COPY --from=build /app/.output/ ./

# Change the port and host
ENV PORT=80
ENV HOST=0.0.0.0

EXPOSE 80

CMD ["node", "/app/server/index.mjs"]

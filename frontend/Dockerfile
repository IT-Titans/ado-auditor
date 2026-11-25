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

# Tests deliberately do not run here. A failing RUN surfaces as a Docker build error
# with no test report and no coverage attached to the build. The pipeline runs them in
# its own Test stage instead, where the results and the coverage are published.

# The install above ran `nuxt prepare` as its postinstall step before the sources were
# copied, so the types and path aliases in .nuxt describe an empty project. Nuxt 4 builds
# in a directory of its own and leaves .nuxt as it is, while the Vue compiler reads the
# aliases from there to resolve imported prop types. Without this step the build fails
# with "Failed to resolve import source".
RUN pnpm nuxt prepare

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

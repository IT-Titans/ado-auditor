# Frontend Development

> **Prerequisites**: Ensure you have `pnpm` installed as your package manager before starting frontend development.  
> To install `pnpm`, visit [pnpm Installation](https://pnpm.io/installation) and follow the instructions specific to your operating system.

## Setup Steps

1. **Navigate to the Frontend Directory**
   After cloning the repository, open your terminal and move into the `frontend` folder:
   ```bash
   cd frontend
   ```
2. **Configure Environment Variables**
   Copy the `.env.sample` file to `.env`:
   ```bash
   cp .env.sample .env
   ```
   Edit the `.env` file and set the `NUXT_API_URL` based on your backend setup:
   - **Local API**:
     ```env
     NUXT_API_URL="http://localhost:5279"
     ```
   - **Dockerized API** (replace `<PORT>` with the actual Docker API port):
     ```env
     NUXT_API_URL="http://localhost:<PORT>"
     ```
3. **Install Dependencies**

   ```bash
   pnpm install
   ```
4. **Start Development Server**
   ```bash
   pnpm dev
   ```
   Open your browser at:
   ```
   http://localhost:3000
   ```

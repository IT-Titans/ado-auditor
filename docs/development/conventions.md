# Conventions

## Git

### Branches

The **main** branch contains the latest, *stable* version that is being deployed to the production environment.

The **dev** branch contains the current work in progress. It aims to be as stable as possible. Changes from this branch will, when *stable*, be integrated into **main**.

**Features** are developed on feature branches: `features/{issue-number}-{short-description}`  

**Bug fixes** are developed on fix branches: `fixes/{issue-number}-{short-description}`

Completed code gets merged back to **dev** via a Pull Request, by squash. The pull request has to pass the checks described in [Where the code lives](repository.md).

### Commits

We use [Conventional Commits](https://www.conventionalcommits.org/), for commit messages and for the title of a pull request, which becomes the commit on **dev**: `{type}({scope}): {message}`, for example `fix(frontend): keep the icon styles outside the CSS layers`.  
Where type is e.g. `feat`, `fix`, `chore`, `refactor`, `docs`, `style`, `test`, `build`, `ci`. The scope is optional. An issue is referenced in the description of the pull request.

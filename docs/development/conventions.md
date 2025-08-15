# Conventions

## Git

### Branches

The **main** branch contains the latest, *stable* version that is being deployed to the production environment.

The **dev** branch contains the current work in progress. It aims to be as stable as possible. Changes from this branch will, when *stable*, be integrated into **main**.

**Features** are developed on feature branches: `features/{issue-number}-{short-description}`  

**Bug fixes** are developed on fix branches: `fixes/{issue-number}-{short-description}`

Completed code gets merged back to **dev** via a Pull Request.

### Commits

We use a basic set of conventional commits.

Commit messages are written as follows: `{semantic:} #{issue-number} {message}`  
Where semantic is e.g. `feat`, `fix`, `chore`, `refactor`, `docs`, `style`, `test`.

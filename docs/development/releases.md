# Releases

Before a release, we need to update:

- the [Release Notes](../releases/index.md)
- the version in the [Directory.Build.props](../../backend/Directory.Build.props)

Both changes reach `dev` through a pull request named `chore(release): {MAJOR}.{MINOR}.{PATCH}`.

Once everything is ready and thoroughly tested in the development environment, a new Pull Request named `Release {MAJOR}.{MINOR}.{PATCH}` is created from `dev` to `main`. It is merged with a merge commit, not by squash, so that `main` keeps the commits of `dev`.

Once the Pull Request is reviewed and merged to `main`, a new tag is created at the new commit on `main` in the format `v{MAJOR}.{MINOR}.{PATCH}`.

The pipeline run on `main` deploys to production. The tag is what tells everyone which source is running, which the licence asks for, so every release needs a new version. See [Where the code lives](repository.md) for how the branches, the checks and the deployment fit together.

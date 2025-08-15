# Releases

Before a release, we need to update:

- the [Release Notes](../releases/index.md)
- the version in the [Directory.Build.props](../../backend/Directory.Build.props)

Once everything is ready and thorougly tested in the development environment, a new Pull Request is created from `dev` to `main`.

Once the Pull Request is reviewed and merged to `main`, a new tag is created at the new commit on `main` in the format `v{MAJOR}.{MINOR}.{PATCH}`.

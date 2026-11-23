# Releases

Before a release, we need to update:

- the [Release Notes](../releases/index.md)
- the version in the [Directory.Build.props](../../backend/Directory.Build.props)

Once everything is ready and thorougly tested in the development environment, a new Pull Request is created from `dev` to `main`.

Once the Pull Request is reviewed and merged to `main`, a new tag is created at the new commit on `main` in the format `v{MAJOR}.{MINOR}.{PATCH}`.

The pipeline run on `main` deploys to production and then publishes the release to the public repository on GitHub, as one commit named after the version and tagged `v{MAJOR}.{MINOR}.{PATCH}`. That is why every release needs a new version. See [The public repository on GitHub](github-mirror.md) for what to do when that last stage is red.

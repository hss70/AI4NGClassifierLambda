# AI4NGClassifierLambda

## Deployment

**Current Method**: AWS CodePipeline (see [CODEPIPELINE.md](CODEPIPELINE.md))

**Legacy Method**: GitHub Actions (deprecated)
- Requires S3 bucket configured in samconfig.toml
- Requires repo secrets: AWS_ACCESS_KEY_ID_DEV, AWS_SECRET_ACCESS_KEY_DEV

## API Documentation

Changes to the API generate a yaml openAPI spec and send it to https://github.com/hss70/AI4NGApiDoc 

API docs hosted on https://hss70.github.io/AI4NGApiDoc/

Doc auto update requires a repo secret DOCS_REPO_PAT (PAT for the docs repo)
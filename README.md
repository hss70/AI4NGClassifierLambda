# AI4NGClassifierLambda

Requires an s3 bucket for deployment to aws. This is configured in samconfig.toml
Also requires:
AWS_ACCESS_KEY_ID_DEV
AWS_SECRET_ACCESS_KEY_DEV
To be added to the repo secrets

Changes to the API generate a yaml openAPI spec and send it to https://github.com/hss70/AI4NGApiDoc 

API docs hosted on https://hss70.github.io/AI4NGApiDoc/

Doc auto update requires a repo secret
DOCS_REPO_PAT
which is the PAT for the docs repo
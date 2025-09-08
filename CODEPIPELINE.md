# CodePipeline Deployment Guide

## Overview
This project uses AWS CodePipeline for automated deployment instead of GitHub Actions.

## Initial Setup

### 1. Create GitHub Token
1. Go to https://github.com/settings/tokens
2. Click "Developer settings" → "Personal access tokens" → "Tokens (classic)"
3. Click "Generate new token (classic)"
4. Configure:
   - **Note**: "CodePipeline access"
   - **Expiration**: 90 days (recommended)
   - **Scopes**: Check `repo` only
5. Copy the token (starts with `ghp_`)

### 2. Deploy Pipeline
```bash
aws cloudformation create-stack \
  --stack-name ai4ng-classifier-pipeline \
  --template-body file://pipeline.yaml \
  --parameters ParameterKey=GitHubToken,ParameterValue=ghp_your_token_here \
  --capabilities CAPABILITY_IAM
```

### 3. Verify Deployment
- Check AWS Console → CodePipeline
- Pipeline should trigger automatically on code push

## How It Works

1. **Source Stage**: Pulls code from GitHub when you push to `main`
2. **Build Stage**: Runs `buildspec.yml` using CodeBuild
   - Installs .NET 8 and SAM CLI
   - Builds Lambda with `sam build`
   - Packages for deployment
3. **DeployDev Stage**: Auto-deploys to dev environment (`AI4NGClassifier-dev`)
4. **ApprovalForProd Stage**: Manual approval gate
5. **DeployProd Stage**: Deploys to production (`AI4NGClassifier-prod`)

## Troubleshooting

### Pipeline Fails at Source Stage
**Cause**: GitHub token expired or invalid

**Fix**:
```bash
# Update the stack with new token
aws cloudformation update-stack \
  --stack-name ai4ng-classifier-pipeline \
  --use-previous-template \
  --parameters ParameterKey=GitHubToken,ParameterValue=ghp_new_token_here \
  --capabilities CAPABILITY_IAM
```

### Build Stage Fails
**Common Issues**:
- Check CodeBuild logs in AWS Console
- Ensure `buildspec.yml` is in repo root
- Verify .NET 8 compatibility

### Deploy Stage Fails
**Common Issues**:
- Check CloudFormation events in AWS Console
- Ensure DynamoDB tables exist for both environments
- Verify IAM permissions

### Manual Approval
**To approve production deployment**:
1. Go to AWS Console → CodePipeline → ai4ng-classifier-pipeline
2. Click "Review" on the ApprovalForProd stage
3. Add optional comments and click "Approve"

## Manual Operations

### Trigger Pipeline Manually
AWS Console → CodePipeline → ai4ng-classifier-pipeline → "Release change"

### View Logs
- **Build logs**: CodeBuild → Build projects → View build history
- **Dev deploy logs**: CloudFormation → Stacks → AI4NGClassifier-dev → Events
- **Prod deploy logs**: CloudFormation → Stacks → AI4NGClassifier-prod → Events

### Delete Pipeline
```bash
aws cloudformation delete-stack --stack-name ai4ng-classifier-pipeline
```

## Files
- `pipeline.yaml` - CodePipeline CloudFormation template
- `buildspec.yml` - CodeBuild instructions
- `infra/ClassifierApi.yaml` - Lambda infrastructure template
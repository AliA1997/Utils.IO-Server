set -e

aws ecr get-login-password --region <aws region where repo is found> --profile <iam user profile> | docker login --username AWS --password-stdin <arn>.dkr.ecr.us-east-2.amazonaws.com/<repo name>
docker build -f ./Dockerfile -t <docker image name>:latest .
docker tag <docker image name>:latest <arn>.dkr.ecr.us-east-2.amazonaws.com/<repo name>:latest
docker push <arn>.dkr.ecr.us-east-2.amazonaws.com/<repo name>:latest
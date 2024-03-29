FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update && apt-get install -y \
    openssh-client \
    tar \
    gzip \
    git \
    nodejs \
    gcc \
    fish \
    npm && \ 
    npm i -g n && \
    n 18
    
RUN groupadd --gid 1000 developer \
    && useradd --uid 1000 --gid 1000 -ms /usr/bin/fish developer \
    && chown -R developer. /home/developer

USER developer

WORKDIR /app
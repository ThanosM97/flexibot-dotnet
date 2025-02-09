FROM mcr.microsoft.com/devcontainers/dotnet:8.0

RUN sudo curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg
RUN sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/dotnetdev.list'

# Fix windows / linux new line issue
RUN git config --global core.autocrlf input
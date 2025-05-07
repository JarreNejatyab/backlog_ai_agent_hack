# Use an official Python runtime as a parent image
FROM mcr.microsoft.com/devcontainers/universal:2

# Set environment variables for proxy
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID
# ARG http_proxy
# ARG https_proxy
# ENV http_proxy=${http_proxy}
# ENV https_proxy=${https_proxy}

# Install Node.js and other dependencies
COPY zscaler-cert.pem /usr/local/share/ca-certificates/zscaler-cert.crt
RUN apt-get update && \
    apt-get install -y curl ca-certificates && \
    update-ca-certificates

# Create group and user, add sudo support & git, configure sudo privileges, and clean up in one RUN command
RUN mkdir -p /etc/sudoers.d \
    && groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME \
    && rm -rf /var/lib/apt/lists/*

USER $USERNAME
CMD ["sleep", "infinity"]

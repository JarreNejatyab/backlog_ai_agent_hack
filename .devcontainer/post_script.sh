#!/bin/bash

# Configuring git SSL for pre-commit hooks
# git config --global http.sslCAInfo /usr/local/share/ca-certificates/zscaler-cert.crt

# Configure git for pushing to origin
git config credential.useHttpPath true

# Define the path to your Zsh profile
zshrc_path="$HOME/.zshrc"
bashrc_path="$HOME/.bashrc"

echo "export PATH=\"$HOME/.local/bin:$PATH\"" >> "$zshrc_path"
echo "export PATH=\"$HOME/.local/bin:$PATH\"" >> "$bashrc_path"

cat "$HOME"/.zshrc
export PATH="$HOME/.local/bin:$PATH"

# Add ca-cert to python
# pip install certifi pytest
# cat zscaler-cert.pem >> /home/vscode/.local/lib/python3.11/site-packages/certifi/cacert.pem
# sudo cat zscaler-cert.pem | sudo tee -a /opt/az/lib/python3.12/site-packages/certifi/cacert.pem > /dev/null

# az extension add --name ml

#!/bin/bash

# Generate SSH key for git operations if not present
SSH_KEY="$HOME/.ssh/id_ed25519"
if [ ! -f "$SSH_KEY" ]; then
  mkdir -p "$HOME/.ssh"
  chmod 700 "$HOME/.ssh"
  ssh-keygen -t ed25519 -C "bld-league-devcontainer" -f "$SSH_KEY" -N ""
  echo ""
  echo "=========================================="
  echo "New SSH key generated."
  echo "Add the public key below to GitHub:"
  echo "https://github.com/settings/ssh/new"
  echo "=========================================="
  cat "${SSH_KEY}.pub"
  echo "=========================================="
fi

# Trust GitHub's host key
ssh-keyscan -H github.com >> "$HOME/.ssh/known_hosts" 2>/dev/null

# Use SSH for all GitHub git operations
git config --global url."git@github.com:".insteadOf "https://github.com/"

# Register GitHub MCP server
if [ -n "$GITHUB_PERSONAL_ACCESS_TOKEN" ]; then
  claude mcp add-json github \
    "{\"type\":\"http\",\"url\":\"https://api.githubcopilot.com/mcp\",\"headers\":{\"Authorization\":\"Bearer $GITHUB_PERSONAL_ACCESS_TOKEN\"}}" \
    --scope user 2>/dev/null || true
fi

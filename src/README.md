# Semantic Kernel - Friendly Agent

This Python application creates a friendly conversational agent using Microsoft's Semantic Kernel library with Azure OpenAI integration. The agent uses the GPT-4o model and allows users to interact through a command-line interface.

## Setup

1. Create a `.env` file in the `src` directory based on the `.env.example` template:
   ```
   cp .env.example .env
   ```

2. Update the `.env` file with your Azure OpenAI credentials:
   ```
   AZURE_OPENAI_API_KEY="your-azure-openai-api-key"
   AZURE_OPENAI_ENDPOINT="https://your-azure-openai-endpoint.openai.azure.com/"
   AZURE_OPENAI_API_VERSION="2024-05-01-preview"
   ```

3. Install the required dependencies:
   ```
   pip install -r requirements.txt
   ```

## Usage

Run the agent using:
```
python agent.py
```

The agent will start a conversation in the terminal where you can chat interactively. Type 'exit' to end the conversation.

## Features

- Uses Azure OpenAI with the GPT-4o model
- Maintains conversation context through threads
- Provides friendly, helpful responses
- Simple command-line interface for interaction

## Requirements

- Python 3.8+
- Azure OpenAI API access with GPT-4o model deployment

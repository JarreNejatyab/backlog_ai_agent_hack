import os
import asyncio
import logging

import chainlit as cl
from dotenv import load_dotenv
from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.agents import ChatCompletionAgent
from semantic_kernel.contents import ChatHistory

# Setup logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Load environment variables
load_dotenv()

# Global agent instance - will be initialized per session
agent = None
chat_history = None

def setup_agent():
    """Set up the Chat Completion Agent with Azure OpenAI."""
    try:
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        deployment_name = "gpt-4o"
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2024-05-01-preview")
        
        instructions = """
        You are a friendly and helpful assistant. Your goal is to have a pleasant conversation
        with the user and assist them with any questions or tasks they have. Be supportive,
        empathetic, and positive in your interactions. Provide clear and concise information, 
        and if you're not sure about something, just be honest about it.
        """
        
        # Initialize the kernel
        kernel = Kernel()
        
        # Set up Azure OpenAI chat service
        chat_service = AzureChatCompletion(
            service_id="chat",
            deployment_name=deployment_name,
            endpoint=endpoint,
            api_key=api_key,
            api_version=api_version
        )
        
        # Add the service to the kernel
        kernel.add_service(chat_service)
        
        # Initialize the chat agent
        agent = ChatCompletionAgent(
            service=chat_service,
            name="backlog-assistant",
            instructions=instructions
        )
        
        # Create chat history
        chat_history = ChatHistory()
        chat_history.add_system_message(instructions)
        
        logger.info("Agent setup completed successfully")
        return agent, chat_history
        
    except Exception as e:
        logger.error(f"Error setting up agent: {str(e)}")
        raise

@cl.on_chat_start
async def on_chat_start():
    """Initialize the agent when a new chat starts."""
    global agent, chat_history
    
    try:
        # Create a new agent instance for this session
        agent, chat_history = setup_agent()
        
        # Send an initial message to the user
        await cl.Message(
            content="Hello! I'm your friendly assistant. How can I help you today?",
            author="Friendly Agent"
        ).send()
        
    except Exception as e:
        logger.error(f"Error in chat start: {str(e)}")
        await cl.Message(
            content=f"Failed to initialize the agent: {str(e)}",
            author="System"
        ).send()

@cl.on_message
async def on_message(message: cl.Message):
    """Process incoming user messages."""
    global agent, chat_history
    
    try:
        # If agent not initialized, set it up
        if not agent or not chat_history:
            agent, chat_history = setup_agent()
        
        # Log the received message to ensure it's being captured
        logger.info(f"Received message: {message.content}")
        
        # Show thinking indicator
        async with cl.Step(name="Thinking...", show_input=True):
            # Add user message to chat history
            chat_history.add_user_message(message.content)
            
            # Get response from agent
            response = await agent.get_response(chat_history=chat_history)
            
            # Add assistant response to chat history to maintain conversation context
            chat_history.add_assistant_message(response.content)
        
        # Send response back to the user
        await cl.Message(
            content=response.content,
            author="Friendly Agent"
        ).send()
    
    except Exception as e:
        logger.error(f"Error processing message: {str(e)}")
        await cl.Message(
            content=f"Sorry, I encountered an error: {str(e)}",
            author="System"
        ).send()

# No need for on_chat_end since we're using global variables
# The variables will be reset when a new session starts

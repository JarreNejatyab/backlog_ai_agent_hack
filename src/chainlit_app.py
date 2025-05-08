import os
import logging
from typing import Dict

import chainlit as cl
from chainlit.context import context as client_context
from chainlit.types import ThreadDict

from agent import FriendlyAgent

# Setup logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Store agent instances for each user session
agent_instances: Dict[str, FriendlyAgent] = {}

@cl.on_chat_start
async def on_chat_start():
    """Initialize the agent when a new chat starts."""
    try:
        # Create a new agent instance for this session
        agent = FriendlyAgent()
        agent_instances[client_context.session_id] = agent
        
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
    try:
        # Get the agent for this session
        agent = agent_instances.get(client_context.session_id)
        if not agent:
            # If agent doesn't exist (session might have expired), create a new one
            agent = FriendlyAgent()
            agent_instances[client_context.session_id] = agent
        
        # Log the incoming message
        logger.info(f"Processing message: {message.content}")
        
        # Show thinking indicator with user message displayed
        async with cl.Step(name="Processing", show_input=True):
            # Get response from agent
            response = await agent.chat(message.content)
        
        # Send response back to the user
        await cl.Message(
            content=response,
            author="Friendly Agent"
        ).send()
    
    except Exception as e:
        logger.error(f"Error processing message: {str(e)}")
        await cl.Message(
            content=f"Sorry, I encountered an error: {str(e)}",
            author="System"
        ).send()

@cl.on_chat_end
async def on_chat_end(thread_dict: ThreadDict):
    """Clean up when the chat session ends."""
    try:
        # Remove the agent instance to free up resources
        session_id = client_context.session_id
        if session_id in agent_instances:
            del agent_instances[session_id]
            logger.info(f"Cleaned up agent for session {session_id}")
    except Exception as e:
        logger.error(f"Error in chat end: {str(e)}")

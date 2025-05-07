import os
import asyncio
import logging
from dotenv import load_dotenv
from semantic_kernel import Kernel
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.agents import ChatCompletionAgent
from semantic_kernel.contents import ChatHistory

# Setup logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class FriendlyAgent:
    def __init__(self):
        """Initialize the friendly agent using Azure OpenAI with gpt-4o."""
        load_dotenv()  # Load environment variables from .env file
        
        self.api_key = os.getenv("AZURE_OPENAI_API_KEY")
        self.endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        self.deployment_name = "gpt-4o"
        self.api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2024-05-01-preview")
        
        self.chat_history = ChatHistory()
        self.agent = None
        self.instructions = """
        You are a friendly and helpful assistant. Your goal is to have a pleasant conversation
        with the user and assist them with any questions or tasks they have. Be supportive,
        empathetic, and positive in your interactions. Provide clear and concise information, 
        and if you're not sure about something, just be honest about it.
        """
        
        self._setup_agent()
        
    def _setup_agent(self):
        """Set up the Chat Completion Agent with Azure OpenAI."""
        try:
            # Initialize the kernel
            kernel = Kernel()
            
            # Set up Azure OpenAI chat service
            chat_service = AzureChatCompletion(
                service_id="chat",
                deployment_name=self.deployment_name,
                endpoint=self.endpoint,
                api_key=self.api_key,
                api_version=self.api_version
            )
            
            # Add the service to the kernel
            kernel.add_service(chat_service)
            
            # Initialize the chat agent
            self.agent = ChatCompletionAgent(
                service=chat_service,
                name="ado-assistant",
                instructions=self.instructions
            )
            
            # Add system message with instructions
            self.chat_history.add_system_message(self.instructions)
            
            logger.info("Agent setup completed successfully")
            
        except Exception as e:
            logger.error(f"Error setting up agent: {str(e)}")
            raise
    
    async def chat(self, message):
        """Chat with the agent and get a response."""
        try:
            if not self.agent:
                raise ValueError("Agent has not been properly initialized")
            
            # Add user message to chat history
            self.chat_history.add_user_message(message)
            
            # Get response from agent
            response = await self.agent.get_response(chat_history=self.chat_history)
            
            return response.content
            
        except Exception as e:
            logger.error(f"Error during chat: {str(e)}")
            return f"I encountered an error: {str(e)}"
            
    def run_conversation(self):
        """Run an interactive conversation with the user via command line."""
        print("Friendly Agent: Hello! I'm your friendly assistant. How can I help you today?")
        print("(Type 'exit' to end the conversation)")
        
        while True:
            user_input = input("You: ")
            
            # Check if user wants to exit
            if user_input.lower() in ["exit", "quit", "bye"]:
                print("Friendly Agent: Goodbye! Have a great day!")
                break
                
            # Get response from agent
            response = asyncio.run(self.chat(user_input))
            print(f"Friendly Agent: {response}")
            
if __name__ == "__main__":
    try:
        agent = FriendlyAgent()
        agent.run_conversation()
    except Exception as e:
        logging.error(f"Error starting the agent: {str(e)}")
        print(f"Error starting the agent: {str(e)}")

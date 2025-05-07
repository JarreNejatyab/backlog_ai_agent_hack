#!/usr/bin/env python3
import os
import sys
import logging

# Add the current directory to the path so we can import from the current directory
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from agent import FriendlyAgent

def main():
    """Main entry point for the application"""
    logging.basicConfig(level=logging.INFO, 
                        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
    
    try:
        print("Initializing friendly agent...")
        agent = FriendlyAgent()
        print("Starting conversation. Type 'exit' to end.")
        agent.run_conversation()
    except KeyboardInterrupt:
        print("\nExiting gracefully...")
    except Exception as e:
        logging.error(f"Error running agent: {str(e)}")
        print(f"An error occurred: {str(e)}")
        return 1
    return 0

if __name__ == "__main__":
    sys.exit(main())

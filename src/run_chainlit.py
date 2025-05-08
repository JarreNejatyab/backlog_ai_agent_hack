
"""
Run script for the Chainlit application.
This script is the entry point for running the Chainlit app.
"""

import os
import sys
import subprocess

# Add the current directory to the path so we can import our modules
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

if __name__ == "__main__":
    print("Starting Chainlit application...")
    
    # Use the app.py by default, but allow overriding
    app_file = "app.py"
    if len(sys.argv) > 1:
        app_file = sys.argv[1]
    
    # Form the command to run chainlit
    cmd = ["chainlit", "run", app_file, "--port", "8000", "--host", "0.0.0.0"]
    
    # Run the command
    try:
        subprocess.run(cmd, check=True)
    except subprocess.CalledProcessError as e:
        print(f"Error running Chainlit: {e}")
        sys.exit(1)
    except KeyboardInterrupt:
        print("Shutting down Chainlit...")
        sys.exit(0)
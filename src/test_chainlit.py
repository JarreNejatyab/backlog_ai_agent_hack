import chainlit as cl

@cl.on_chat_start
async def chat_start():
    await cl.Message(content="Hello there!").send()
    
@cl.on_message
async def on_message(message):
    await cl.Message(content=f"You said: {message.content}").send()


To get this working, follow these steps:
First, build and publish the agent: Release
Create the necessary directories for logs: LibrarianAgent
Copy the plist file to the LaunchAgents directory: /
Load the launch agent:
plist
To manage the launch agent, you can use these commands:
Start: launchctl start com.librarian.agent
Stop: launchctl stop com.librarian.agent
Unload: launchctl unload ~/Library/LaunchAgents/com.librarian.agent.plist
The launch agent will:
Start automatically when you log in
Restart automatically if it crashes
Log output to ~/Library/Logs/LibrarianAgent/
Run every 5 minutes (you can adjust this interval in the ExecuteAsync method)
The service is currently set up as a template that logs its activity. 
You can modify the ExecuteAsync method in Program.cs to add your specific background processing logic for the Librarian application.